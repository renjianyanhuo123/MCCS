namespace MCCS.Infrastructure.Communication.NamedPipe.Models;

/// <summary>
/// 管道消息类型
/// </summary>
public enum PipeMessageType
{
    /// <summary>
    /// 请求消息（请求-响应模式）
    /// </summary>
    Request = 0,

    /// <summary>
    /// 响应消息（请求-响应模式）
    /// </summary>
    Response = 1,

    /// <summary>
    /// 订阅消息（发布-订阅模式）
    /// </summary>
    Subscribe = 2,

    /// <summary>
    /// 取消订阅消息（发布-订阅模式）
    /// </summary>
    Unsubscribe = 3,

    /// <summary>
    /// 发布消息（发布-订阅模式）
    /// </summary>
    Publish = 4,

    /// <summary>
    /// 订阅确认消息
    /// </summary>
    SubscribeAck = 5,

    /// <summary>
    /// 取消订阅确认消息
    /// </summary>
    UnsubscribeAck = 6
}

/// <summary>
/// 发布-订阅消息基类
/// </summary>
public abstract class PubSubMessageBase
{
    /// <summary>
    /// 消息ID
    /// </summary>
    public string MessageId { get; set; } = Guid.NewGuid().ToString("N");

    /// <summary>
    /// 消息类型
    /// </summary>
    public abstract PipeMessageType MessageType { get; }

    /// <summary>
    /// 消息时间戳（UTC毫秒）
    /// </summary>
    public long Timestamp { get; set; } = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
}

/// <summary>
/// 订阅请求消息
/// </summary>
public sealed class SubscribeMessage : PubSubMessageBase
{
    public override PipeMessageType MessageType => PipeMessageType.Subscribe;

    /// <summary>
    /// 订阅的主题
    /// </summary>
    public string Topic { get; set; } = string.Empty;

    /// <summary>
    /// 订阅者ID（由服务端分配或客户端指定）
    /// </summary>
    public string? SubscriberId { get; set; }

    /// <summary>
    /// 创建订阅消息
    /// </summary>
    public static SubscribeMessage Create(string topic, string? subscriberId = null)
    {
        return new SubscribeMessage
        {
            Topic = topic,
            SubscriberId = subscriberId
        };
    }
}

/// <summary>
/// 取消订阅消息
/// </summary>
public sealed class UnsubscribeMessage : PubSubMessageBase
{
    public override PipeMessageType MessageType => PipeMessageType.Unsubscribe;

    /// <summary>
    /// 取消订阅的主题（为空则取消所有订阅）
    /// </summary>
    public string? Topic { get; set; }

    /// <summary>
    /// 订阅者ID
    /// </summary>
    public string? SubscriberId { get; set; }

    /// <summary>
    /// 创建取消订阅消息
    /// </summary>
    public static UnsubscribeMessage Create(string? topic = null, string? subscriberId = null)
    {
        return new UnsubscribeMessage
        {
            Topic = topic,
            SubscriberId = subscriberId
        };
    }
}

/// <summary>
/// 发布消息
/// </summary>
public sealed class PublishMessage : PubSubMessageBase
{
    public override PipeMessageType MessageType => PipeMessageType.Publish;

    /// <summary>
    /// 发布的主题
    /// </summary>
    public string Topic { get; set; } = string.Empty;

    /// <summary>
    /// 消息负载（JSON格式）
    /// </summary>
    public string? Payload { get; set; }

    /// <summary>
    /// 创建发布消息
    /// </summary>
    public static PublishMessage Create(string topic, string? payload = null)
    {
        return new PublishMessage
        {
            Topic = topic,
            Payload = payload
        };
    }

    /// <summary>
    /// 创建带泛型数据的发布消息
    /// </summary>
    public static PublishMessage Create<T>(string topic, T data, Func<T, string> serializer)
    {
        return new PublishMessage
        {
            Topic = topic,
            Payload = serializer(data)
        };
    }
}

/// <summary>
/// 订阅确认消息
/// </summary>
public sealed class SubscribeAckMessage : PubSubMessageBase
{
    public override PipeMessageType MessageType => PipeMessageType.SubscribeAck;

    /// <summary>
    /// 是否成功
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// 订阅的主题
    /// </summary>
    public string Topic { get; set; } = string.Empty;

    /// <summary>
    /// 分配的订阅者ID
    /// </summary>
    public string SubscriberId { get; set; } = string.Empty;

    /// <summary>
    /// 错误消息（如果失败）
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// 创建成功确认
    /// </summary>
    public static SubscribeAckMessage Succeeded(string topic, string subscriberId, string? messageId = null)
    {
        return new SubscribeAckMessage
        {
            MessageId = messageId ?? Guid.NewGuid().ToString("N"),
            Success = true,
            Topic = topic,
            SubscriberId = subscriberId
        };
    }

    /// <summary>
    /// 创建失败确认
    /// </summary>
    public static SubscribeAckMessage Failed(string topic, string errorMessage, string? messageId = null)
    {
        return new SubscribeAckMessage
        {
            MessageId = messageId ?? Guid.NewGuid().ToString("N"),
            Success = false,
            Topic = topic,
            ErrorMessage = errorMessage
        };
    }
}

/// <summary>
/// 取消订阅确认消息
/// </summary>
public sealed class UnsubscribeAckMessage : PubSubMessageBase
{
    public override PipeMessageType MessageType => PipeMessageType.UnsubscribeAck;

    /// <summary>
    /// 是否成功
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// 取消订阅的主题
    /// </summary>
    public string? Topic { get; set; }

    /// <summary>
    /// 错误消息（如果失败）
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// 创建成功确认
    /// </summary>
    public static UnsubscribeAckMessage Succeeded(string? topic = null, string? messageId = null)
    {
        return new UnsubscribeAckMessage
        {
            MessageId = messageId ?? Guid.NewGuid().ToString("N"),
            Success = true,
            Topic = topic
        };
    }

    /// <summary>
    /// 创建失败确认
    /// </summary>
    public static UnsubscribeAckMessage Failed(string errorMessage, string? topic = null, string? messageId = null)
    {
        return new UnsubscribeAckMessage
        {
            MessageId = messageId ?? Guid.NewGuid().ToString("N"),
            Success = false,
            Topic = topic,
            ErrorMessage = errorMessage
        };
    }
}

/// <summary>
/// 通用管道消息包装器（用于传输层）
/// </summary>
public sealed class PipeMessageEnvelope
{
    /// <summary>
    /// 消息类型
    /// </summary>
    public PipeMessageType MessageType { get; set; }

    /// <summary>
    /// 消息负载（JSON格式的具体消息）
    /// </summary>
    public string Payload { get; set; } = string.Empty;

    /// <summary>
    /// 创建消息包装器
    /// </summary>
    public static PipeMessageEnvelope Create(PipeMessageType messageType, string payload)
    {
        return new PipeMessageEnvelope
        {
            MessageType = messageType,
            Payload = payload
        };
    }
}
