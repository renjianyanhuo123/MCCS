using System.Reflection;
using MCCS.Infrastructure.Communication.NamedPipe.Handlers;
using MCCS.Infrastructure.Communication.NamedPipe.Serialization;
using Serilog;

namespace MCCS.Infrastructure.Communication.NamedPipe;

/// <summary>
/// 命名管道工厂 - 简化创建服务端和客户端
/// </summary>
public static class NamedPipeFactory
{
    /// <summary>
    /// 创建服务端
    /// </summary>
    /// <param name="pipeName">管道名称</param>
    /// <param name="maxConnections">最大连接数</param>
    /// <param name="logger">日志记录器</param>
    /// <returns>服务端实例</returns>
    public static NamedPipeServer CreateServer(
        string pipeName = "MCCS_IPC_Pipe",
        int maxConnections = 10,
        ILogger? logger = null)
    {
        var options = new NamedPipeServerOptions
        {
            PipeName = pipeName,
            MaxConcurrentConnections = maxConnections
        };

        return new NamedPipeServer(options, new JsonMessageSerializer(), logger);
    }

    /// <summary>
    /// 通过特性扫描创建服务端并注册处理器
    /// </summary>
    /// <param name="maxConnections">最大连接数</param>
    /// <param name="logger">日志记录器</param>
    /// <param name="assemblies">扫描程序集</param>
    /// <returns>服务端实例</returns>
    public static NamedPipeServer CreateServerFromAttributes(
        int maxConnections = 10,
        ILogger? logger = null,
        params Assembly[] assemblies)
    {
        var resolvedAssemblies = assemblies.Length == 0
            ? new[] { Assembly.GetCallingAssembly() }
            : assemblies;

        var pipeName = AttributedHandlerRegistrar.ResolvePipeName(resolvedAssemblies);
        var server = CreateServer(pipeName, maxConnections, logger);
        server.RegisterHandlersFromAttributes(resolvedAssemblies);
        return server;
    }

    /// <summary>
    /// 创建服务端（使用配置）
    /// </summary>
    /// <param name="configure">配置委托</param>
    /// <param name="logger">日志记录器</param>
    /// <returns>服务端实例</returns>
    public static NamedPipeServer CreateServer(
        Action<NamedPipeServerOptions> configure,
        ILogger? logger = null)
    {
        var options = new NamedPipeServerOptions();
        configure(options);
        return new NamedPipeServer(options, new JsonMessageSerializer(), logger);
    }

    /// <summary>
    /// 创建客户端
    /// </summary>
    /// <param name="pipeName">管道名称</param>
    /// <param name="logger">日志记录器</param>
    /// <returns>客户端实例</returns>
    public static NamedPipeClient CreateClient(
        string pipeName = "MCCS_IPC_Pipe",
        ILogger? logger = null)
    {
        var options = new NamedPipeClientOptions
        {
            PipeName = pipeName
        };

        return new NamedPipeClient(options, new JsonMessageSerializer(), logger);
    }

    /// <summary>
    /// 创建客户端（使用配置）
    /// </summary>
    /// <param name="configure">配置委托</param>
    /// <param name="logger">日志记录器</param>
    /// <returns>客户端实例</returns>
    public static NamedPipeClient CreateClient(
        Action<NamedPipeClientOptions> configure,
        ILogger? logger = null)
    {
        var options = new NamedPipeClientOptions();
        configure(options);
        return new NamedPipeClient(options, new JsonMessageSerializer(), logger);
    }

    /// <summary>
    /// 创建客户端连接池
    /// </summary>
    /// <param name="pipeName">管道名称</param>
    /// <param name="maxConnections">最大连接数</param>
    /// <param name="logger">日志记录器</param>
    /// <returns>客户端连接池实例</returns>
    public static NamedPipeClientPool CreateClientPool(
        string pipeName = "MCCS_IPC_Pipe",
        int maxConnections = 10,
        ILogger? logger = null)
    {
        var options = new NamedPipeClientPoolOptions
        {
            PipeName = pipeName,
            MaxConnections = maxConnections
        };

        return new NamedPipeClientPool(options, new JsonMessageSerializer(), logger);
    }

    /// <summary>
    /// 创建客户端连接池（使用配置）
    /// </summary>
    /// <param name="configure">配置委托</param>
    /// <param name="logger">日志记录器</param>
    /// <returns>客户端连接池实例</returns>
    public static NamedPipeClientPool CreateClientPool(
        Action<NamedPipeClientPoolOptions> configure,
        ILogger? logger = null)
    {
        var options = new NamedPipeClientPoolOptions();
        configure(options);
        return new NamedPipeClientPool(options, new JsonMessageSerializer(), logger);
    }
}

/// <summary>
/// 扩展方法
/// </summary>
public static class NamedPipeExtensions
{
    /// <summary>
    /// 通过特性扫描注册处理器
    /// </summary>
    /// <param name="server">服务端实例</param>
    /// <param name="assemblies">扫描程序集</param>
    /// <returns>服务端实例</returns>
    public static NamedPipeServer RegisterHandlersFromAttributes(this NamedPipeServer server, params Assembly[] assemblies)
    {
        var resolvedAssemblies = assemblies.Length == 0
            ? new[] { Assembly.GetCallingAssembly() }
            : assemblies;

        AttributedHandlerRegistrar.RegisterHandlers(server, resolvedAssemblies, server.Serializer);
        return server;
    }

    /// <summary>
    /// 获取响应负载并反序列化
    /// </summary>
    /// <typeparam name="T">目标类型</typeparam>
    /// <param name="response">响应消息</param>
    /// <param name="serializer">序列化器</param>
    /// <returns>反序列化后的对象</returns>
    public static T? GetPayload<T>(this Models.PipeResponse response, IMessageSerializer? serializer = null)
    {
        if (!response.IsSuccess || string.IsNullOrEmpty(response.Payload))
        {
            return default;
        }

        serializer ??= new JsonMessageSerializer();
        return serializer.Deserialize<T>(response.Payload);
    }

    /// <summary>
    /// 设置请求负载
    /// </summary>
    /// <typeparam name="T">数据类型</typeparam>
    /// <param name="request">请求消息</param>
    /// <param name="data">数据</param>
    /// <param name="serializer">序列化器</param>
    /// <returns>请求消息</returns>
    public static Models.PipeRequest SetPayload<T>(this Models.PipeRequest request, T data, IMessageSerializer? serializer = null)
    {
        serializer ??= new JsonMessageSerializer();
        request.Payload = serializer.Serialize(data);
        return request;
    }
}
