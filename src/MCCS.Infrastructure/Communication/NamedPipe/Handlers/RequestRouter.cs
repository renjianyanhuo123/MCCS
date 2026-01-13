using System.Collections.Concurrent;
using MCCS.Infrastructure.Communication.NamedPipe.Models;

namespace MCCS.Infrastructure.Communication.NamedPipe.Handlers;

/// <summary>
/// 请求路由器 - 负责将请求分发到对应的处理器
/// </summary>
public sealed class RequestRouter
{
    private readonly ConcurrentDictionary<string, IRequestHandler> _handlers = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// 注册处理器
    /// </summary>
    /// <param name="handler">处理器实例</param>
    public void RegisterHandler(IRequestHandler handler)
    {
        if (!_handlers.TryAdd(handler.Route, handler))
        {
            throw new InvalidOperationException($"Handler for route '{handler.Route}' is already registered");
        }
    }

    /// <summary>
    /// 注册委托处理器
    /// </summary>
    /// <param name="route">路由</param>
    /// <param name="handler">处理委托</param>
    public void RegisterHandler(string route, Func<PipeRequest, CancellationToken, Task<PipeResponse>> handler)
    {
        RegisterHandler(new DelegateRequestHandler(route, handler));
    }

    /// <summary>
    /// 注册同步委托处理器
    /// </summary>
    /// <param name="route">路由</param>
    /// <param name="handler">处理委托</param>
    public void RegisterHandler(string route, Func<PipeRequest, PipeResponse> handler)
    {
        RegisterHandler(new DelegateRequestHandler(route, handler));
    }

    /// <summary>
    /// 移除处理器
    /// </summary>
    /// <param name="route">路由</param>
    /// <returns>是否成功移除</returns>
    public bool UnregisterHandler(string route)
    {
        return _handlers.TryRemove(route, out _);
    }

    /// <summary>
    /// 检查是否存在处理器
    /// </summary>
    /// <param name="route">路由</param>
    /// <returns>是否存在</returns>
    public bool HasHandler(string route)
    {
        return _handlers.ContainsKey(route);
    }

    /// <summary>
    /// 获取所有已注册的路由
    /// </summary>
    public IEnumerable<string> GetRegisteredRoutes()
    {
        return _handlers.Keys;
    }

    /// <summary>
    /// 路由并处理请求
    /// </summary>
    /// <param name="request">请求消息</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>响应消息</returns>
    public async Task<PipeResponse> RouteAsync(PipeRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(request.Route))
        {
            return PipeResponse.Failure(request.RequestId, PipeStatusCode.InvalidRequest, "Route is empty");
        }

        if (!_handlers.TryGetValue(request.Route, out var handler))
        {
            return PipeResponse.Failure(request.RequestId, PipeStatusCode.HandlerNotFound, $"No handler found for route '{request.Route}'");
        }

        var startTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        try
        {
            var response = await handler.HandleAsync(request, cancellationToken);
            response.ProcessingTimeMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - startTime;
            return response;
        }
        catch (OperationCanceledException)
        {
            return PipeResponse.Failure(request.RequestId, PipeStatusCode.Timeout, "Request processing was cancelled");
        }
        catch (Exception ex)
        {
            var response = PipeResponse.FromException(request.RequestId, ex);
            response.ProcessingTimeMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - startTime;
            return response;
        }
    }

    /// <summary>
    /// 清空所有处理器
    /// </summary>
    public void Clear()
    {
        _handlers.Clear();
    }
}
