using System.Reflection;

using MCCS.Infrastructure.Communication.NamedPipe.Models;
using MCCS.Infrastructure.Communication.NamedPipe.Serialization;

namespace MCCS.Infrastructure.Communication.NamedPipe.Handlers;

internal static class AttributedHandlerRegistrar
{
    /// <summary>
    /// 扫描所有的 ApiNamedPipeAttribute 特性以解析管道名称
    /// </summary>
    /// <param name="assemblies"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public static string ResolvePipeName(IEnumerable<Assembly> assemblies)
    {
        var pipeNames = assemblies
            .SelectMany(assembly => assembly.DefinedTypes)
            .Select(typeInfo => typeInfo.GetCustomAttribute<ApiNamedPipeAttribute>())
            .Where(attribute => attribute != null)
            .Select(attribute => attribute!.Name)
            .Distinct(StringComparer.Ordinal)
            .ToArray();

        return pipeNames.Length switch
        {
            0 => throw new InvalidOperationException("No ApiNamedPipeAttribute found in provided assemblies."),
            > 1 => throw new InvalidOperationException(
                "Multiple ApiNamedPipeAttribute names found in provided assemblies."),
            _ => pipeNames[0]
        };
    }

    public static void RegisterHandlers(
        NamedPipeServer server,
        IEnumerable<Assembly> assemblies,
        IMessageSerializer serializer)
    {
        var pipeName = server.PipeName;
        foreach (var typeInfo in assemblies.SelectMany(assembly => assembly.DefinedTypes))
        {
            var pipeAttribute = typeInfo.GetCustomAttribute<ApiNamedPipeAttribute>();
            if (pipeAttribute == null || !string.Equals(pipeAttribute.Name, pipeName, StringComparison.Ordinal))
            {
                continue;
            }

            if (typeInfo.IsAbstract)
            {
                continue;
            }

            var instance = Activator.CreateInstance(typeInfo.AsType());
            if (instance == null)
            {
                throw new InvalidOperationException($"Failed to create handler instance for {typeInfo.FullName}.");
            }

            foreach (var method in typeInfo.DeclaredMethods)
            {
                var routeAttribute = method.GetCustomAttribute<RouteAttribute>();
                if (routeAttribute == null) continue;
                var handler = BuildHandler(method, instance, serializer);
                server.RegisterHandler(routeAttribute.Template, handler);
            }
        }
    }

    private static Func<PipeRequest, CancellationToken, Task<PipeResponse>> BuildHandler(
        MethodInfo method,
        object instance,
        IMessageSerializer serializer)
    {
        var parameters = method.GetParameters();
        var hasCancellation = parameters.Length > 0 && parameters[^1].ParameterType == typeof(CancellationToken);
        var payloadParameterCount = parameters.Length - (hasCancellation ? 1 : 0);

        if (payloadParameterCount is < 0 or > 1)
        {
            throw new InvalidOperationException($"Unsupported handler signature: {method.DeclaringType?.FullName}.{method.Name}.");
        }

        var payloadType = payloadParameterCount == 1 ? parameters[0].ParameterType : null;
        var cancellationOnly = payloadType == null && hasCancellation;
        var returnType = method.ReturnType;

        return (request, cancellationToken) =>
        {
            object? payload = null;
            if (payloadType != null)
            {
                if (payloadType == typeof(PipeRequest))
                {
                    payload = request;
                }
                else
                {
                    if (string.IsNullOrEmpty(request.Payload))
                    {
                        return Task.FromResult(PipeResponse.Failure(
                            request.RequestId,
                            PipeStatusCode.InvalidRequest,
                            "Request payload is empty"));
                    }

                    payload = DeserializePayload(serializer, payloadType, request.Payload);
                    if (payload == null)
                    {
                        return Task.FromResult(PipeResponse.Failure(
                            request.RequestId,
                            PipeStatusCode.SerializationError,
                            "Failed to deserialize request payload"));
                    }
                }
            }

            if (returnType == typeof(Task<PipeResponse>))
            {
                if (payloadType == null)
                {
                    if (cancellationOnly)
                    {
                        var delTemp = (Func<CancellationToken, Task<PipeResponse>>)method.CreateDelegate(
                            typeof(Func<CancellationToken, Task<PipeResponse>>),
                            instance);
                        return delTemp(cancellationToken);
                    }

                    var del = (Func<Task<PipeResponse>>)method.CreateDelegate(typeof(Func<Task<PipeResponse>>), instance);
                    return del();
                }

                if (hasCancellation)
                {
                    return (Task<PipeResponse>)method.Invoke(instance, new[] { payload, cancellationToken })!;
                }

                return (Task<PipeResponse>)method.Invoke(instance, new[] { payload })!;
            }

            if (returnType == typeof(PipeResponse))
            {
                if (payloadType == null)
                {
                    if (cancellationOnly)
                    {
                        var delTemp = (Func<CancellationToken, PipeResponse>)method.CreateDelegate(typeof(Func<CancellationToken, PipeResponse>), instance);
                        return Task.FromResult(delTemp(cancellationToken));
                    }

                    var del = (Func<PipeResponse>)method.CreateDelegate(typeof(Func<PipeResponse>), instance);
                    return Task.FromResult(del());
                }

                var result = method.Invoke(instance, hasCancellation ? [payload, cancellationToken] : [payload]);
                return Task.FromResult((PipeResponse)result!);
            }

            if (returnType == typeof(Task))
            {
                return HandleVoidTask(method, instance, payload, hasCancellation, cancellationToken, request.RequestId);
            }

            if (returnType.IsGenericType && returnType.GetGenericTypeDefinition() == typeof(Task<>))
            {
                return HandleTaskResult(
                    method,
                    instance,
                    payload,
                    hasCancellation,
                    cancellationToken,
                    request.RequestId,
                    serializer);
            }

            var resultValue = payloadType == null
                ? (cancellationOnly ? method.Invoke(instance, new object[] { cancellationToken }) : method.Invoke(instance, Array.Empty<object>()))
                : method.Invoke(instance, hasCancellation ? new[] { payload, cancellationToken } : new[] { payload });

            return Task.FromResult(PipeResponse.Success(
                request.RequestId,
                resultValue == null ? null : serializer.Serialize(resultValue)));
        };
    }

    private static object? DeserializePayload(IMessageSerializer serializer, Type payloadType, string payload)
    {
        var method = typeof(IMessageSerializer).GetMethod(nameof(IMessageSerializer.Deserialize))!;
        var generic = method.MakeGenericMethod(payloadType);
        return generic.Invoke(serializer, new object[] { payload });
    }

    private static Task<PipeResponse> HandleVoidTask(
        MethodInfo method,
        object instance,
        object? payload,
        bool hasCancellation,
        CancellationToken cancellationToken,
        string requestId)
    {
        Task task;
        if (payload == null)
        {
            if (hasCancellation)
            {
                var del = (Func<CancellationToken, Task>)method.CreateDelegate(typeof(Func<CancellationToken, Task>), instance);
                task = del(cancellationToken);
            }
            else
            {
                var del = (Func<Task>)method.CreateDelegate(typeof(Func<Task>), instance);
                task = del();
            }
        }
        else
        {
            task = (Task)method.Invoke(instance, hasCancellation ? [payload, cancellationToken] : [payload])!;
        }

        return task.ContinueWith(
            _ => PipeResponse.Success(requestId),
            cancellationToken,
            TaskContinuationOptions.ExecuteSynchronously,
            TaskScheduler.Default);
    }

    private static Task<PipeResponse> HandleTaskResult(
        MethodInfo method,
        object instance,
        object? payload,
        bool hasCancellation,
        CancellationToken cancellationToken,
        string requestId,
        IMessageSerializer serializer)
    {
        var task = payload == null
            ? (Task)method.Invoke(instance, hasCancellation ? [cancellationToken] : [])!
            : (Task)method.Invoke(instance, hasCancellation ? [payload, cancellationToken] : [payload])!;

        return AwaitTaskResult(task, requestId, serializer);
    }

    private static async Task<PipeResponse> AwaitTaskResult(
        Task task,
        string requestId,
        IMessageSerializer serializer)
    {
        await task.ConfigureAwait(false);
        var resultProperty = task.GetType().GetProperty("Result");
        var result = resultProperty?.GetValue(task);
        return result == null ? PipeResponse.Success(requestId) : PipeResponse.Success(requestId, serializer.Serialize(result));
    }
}
