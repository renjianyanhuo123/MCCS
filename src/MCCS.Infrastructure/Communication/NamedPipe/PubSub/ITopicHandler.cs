using MCCS.Infrastructure.Communication.NamedPipe.Models;

namespace MCCS.Infrastructure.Communication.NamedPipe.PubSub;

/// <summary>
/// 主题消息处理器接口
/// </summary>
public interface ITopicHandler
{
    /// <summary>
    /// 处理器支持的主题
    /// </summary>
    string Topic { get; }

    /// <summary>
    /// 处理消息
    /// </summary>
    /// <param name="message">发布的消息</param>
    /// <param name="cancellationToken">取消令牌</param>
    Task HandleAsync(PublishMessage message, CancellationToken cancellationToken = default);
}

/// <summary>
/// 泛型主题消息处理器接口
/// </summary>
/// <typeparam name="T">消息数据类型</typeparam>
public interface ITopicHandler<T> : ITopicHandler
{
    /// <summary>
    /// 处理强类型消息
    /// </summary>
    Task HandleAsync(T data, CancellationToken cancellationToken = default);
}

/// <summary>
/// 主题消息处理器基类
/// </summary>
public abstract class TopicHandlerBase : ITopicHandler
{
    public abstract string Topic { get; }

    public abstract Task HandleAsync(PublishMessage message, CancellationToken cancellationToken = default);
}

/// <summary>
/// 泛型主题消息处理器基类
/// </summary>
/// <typeparam name="T">消息数据类型</typeparam>
public abstract class TopicHandlerBase<T> : ITopicHandler<T>
{
    private readonly Func<string?, T?> _deserializer;

    protected TopicHandlerBase(Func<string?, T?> deserializer)
    {
        _deserializer = deserializer;
    }

    public abstract string Topic { get; }

    public abstract Task HandleAsync(T data, CancellationToken cancellationToken = default);

    public async Task HandleAsync(PublishMessage message, CancellationToken cancellationToken = default)
    {
        var data = _deserializer(message.Payload);
        if (data != null)
        {
            await HandleAsync(data, cancellationToken);
        }
    }
}

/// <summary>
/// 委托主题处理器
/// </summary>
public sealed class DelegateTopicHandler : ITopicHandler
{
    private readonly Func<PublishMessage, CancellationToken, Task> _handler;

    public string Topic { get; }

    public DelegateTopicHandler(string topic, Func<PublishMessage, CancellationToken, Task> handler)
    {
        Topic = topic;
        _handler = handler;
    }

    public DelegateTopicHandler(string topic, Action<PublishMessage> handler)
    {
        Topic = topic;
        _handler = (msg, _) =>
        {
            handler(msg);
            return Task.CompletedTask;
        };
    }

    public Task HandleAsync(PublishMessage message, CancellationToken cancellationToken = default)
    {
        return _handler(message, cancellationToken);
    }
}
