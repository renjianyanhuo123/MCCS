using System.IO.Pipes;

namespace MCCS.Infrastructure.Communication.NamedPipe.PubSub;

/// <summary>
/// 订阅信息
/// </summary>
public sealed class SubscriptionInfo
{
    /// <summary>
    /// 订阅者ID
    /// </summary>
    public string SubscriberId { get; }

    /// <summary>
    /// 订阅的主题
    /// </summary>
    public string Topic { get; }

    /// <summary>
    /// 订阅时间
    /// </summary>
    public DateTimeOffset SubscribedAt { get; }

    /// <summary>
    /// 关联的管道流
    /// </summary>
    internal NamedPipeServerStream? PipeStream { get; set; }

    /// <summary>
    /// 写入锁
    /// </summary>
    internal SemaphoreSlim WriteLock { get; } = new(1, 1);

    public SubscriptionInfo(string subscriberId, string topic)
    {
        SubscriberId = subscriberId;
        Topic = topic;
        SubscribedAt = DateTimeOffset.UtcNow;
    }
}

/// <summary>
/// 订阅管理器接口
/// </summary>
public interface ISubscriptionManager
{
    /// <summary>
    /// 添加订阅
    /// </summary>
    /// <param name="subscriberId">订阅者ID</param>
    /// <param name="topic">主题</param>
    /// <param name="pipeStream">关联的管道流</param>
    /// <returns>订阅信息</returns>
    SubscriptionInfo Subscribe(string subscriberId, string topic, NamedPipeServerStream pipeStream);

    /// <summary>
    /// 取消订阅
    /// </summary>
    /// <param name="subscriberId">订阅者ID</param>
    /// <param name="topic">主题（为空则取消所有订阅）</param>
    /// <returns>是否成功取消</returns>
    bool Unsubscribe(string subscriberId, string? topic = null);

    /// <summary>
    /// 移除订阅者的所有订阅（通常在连接断开时调用）
    /// </summary>
    /// <param name="subscriberId">订阅者ID</param>
    void RemoveSubscriber(string subscriberId);

    /// <summary>
    /// 获取指定主题的所有订阅者
    /// </summary>
    /// <param name="topic">主题</param>
    /// <returns>订阅信息列表</returns>
    IReadOnlyList<SubscriptionInfo> GetSubscribers(string topic);

    /// <summary>
    /// 获取指定订阅者的所有订阅
    /// </summary>
    /// <param name="subscriberId">订阅者ID</param>
    /// <returns>订阅的主题列表</returns>
    IReadOnlyList<string> GetSubscriptions(string subscriberId);

    /// <summary>
    /// 检查指定订阅者是否订阅了某主题
    /// </summary>
    /// <param name="subscriberId">订阅者ID</param>
    /// <param name="topic">主题</param>
    /// <returns>是否已订阅</returns>
    bool IsSubscribed(string subscriberId, string topic);

    /// <summary>
    /// 获取所有主题
    /// </summary>
    /// <returns>主题列表</returns>
    IReadOnlyList<string> GetAllTopics();

    /// <summary>
    /// 获取指定主题的订阅者数量
    /// </summary>
    /// <param name="topic">主题</param>
    /// <returns>订阅者数量</returns>
    int GetSubscriberCount(string topic);

    /// <summary>
    /// 清空所有订阅
    /// </summary>
    void Clear();
}
