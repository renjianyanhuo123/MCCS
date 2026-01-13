using MCCS.Infrastructure.Communication.NamedPipe.Models;

namespace MCCS.Infrastructure.Communication.NamedPipe.Handlers;

/// <summary>
/// 请求处理器接口
/// </summary>
public interface IRequestHandler
{
    /// <summary>
    /// 处理器支持的路由
    /// </summary>
    string Route { get; }

    /// <summary>
    /// 处理请求
    /// </summary>
    /// <param name="request">请求消息</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>响应消息</returns>
    Task<PipeResponse> HandleAsync(PipeRequest request, CancellationToken cancellationToken = default);
}

/// <summary>
/// 泛型请求处理器接口
/// </summary>
/// <typeparam name="TRequest">请求数据类型</typeparam>
/// <typeparam name="TResponse">响应数据类型</typeparam>
public interface IRequestHandler<TRequest, TResponse> : IRequestHandler
{
    /// <summary>
    /// 处理强类型请求
    /// </summary>
    Task<TResponse> HandleAsync(TRequest request, CancellationToken cancellationToken = default);
}

/// <summary>
/// 请求处理器基类
/// </summary>
public abstract class RequestHandlerBase : IRequestHandler
{
    public abstract string Route { get; }

    public abstract Task<PipeResponse> HandleAsync(PipeRequest request, CancellationToken cancellationToken = default);
}

/// <summary>
/// 泛型请求处理器基类
/// </summary>
/// <typeparam name="TRequest">请求数据类型</typeparam>
/// <typeparam name="TResponse">响应数据类型</typeparam>
public abstract class RequestHandlerBase<TRequest, TResponse> : IRequestHandler<TRequest, TResponse>
{
    private readonly Func<string, TRequest?> _deserializer;
    private readonly Func<TResponse, string> _serializer;

    protected RequestHandlerBase(
        Func<string, TRequest?> deserializer,
        Func<TResponse, string> serializer)
    {
        _deserializer = deserializer;
        _serializer = serializer;
    }

    public abstract string Route { get; }

    public abstract Task<TResponse> HandleAsync(TRequest request, CancellationToken cancellationToken = default);

    public async Task<PipeResponse> HandleAsync(PipeRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrEmpty(request.Payload))
            {
                return PipeResponse.Failure(request.RequestId, PipeStatusCode.InvalidRequest, "Request payload is empty");
            }

            var typedRequest = _deserializer(request.Payload);
            if (typedRequest == null)
            {
                return PipeResponse.Failure(request.RequestId, PipeStatusCode.SerializationError, "Failed to deserialize request payload");
            }

            var result = await HandleAsync(typedRequest, cancellationToken);
            var responsePayload = _serializer(result);

            return PipeResponse.Success(request.RequestId, responsePayload);
        }
        catch (OperationCanceledException)
        {
            return PipeResponse.Failure(request.RequestId, PipeStatusCode.Timeout, "Request was cancelled");
        }
        catch (Exception ex)
        {
            return PipeResponse.FromException(request.RequestId, ex);
        }
    }
}

/// <summary>
/// 简单委托处理器（用于快速注册处理逻辑）
/// </summary>
public sealed class DelegateRequestHandler : IRequestHandler
{
    private readonly Func<PipeRequest, CancellationToken, Task<PipeResponse>> _handler;

    public string Route { get; }

    public DelegateRequestHandler(string route, Func<PipeRequest, CancellationToken, Task<PipeResponse>> handler)
    {
        Route = route;
        _handler = handler;
    }

    public DelegateRequestHandler(string route, Func<PipeRequest, PipeResponse> handler)
    {
        Route = route;
        _handler = (req, _) => Task.FromResult(handler(req));
    }

    public Task<PipeResponse> HandleAsync(PipeRequest request, CancellationToken cancellationToken = default)
    {
        return _handler(request, cancellationToken);
    }
}
