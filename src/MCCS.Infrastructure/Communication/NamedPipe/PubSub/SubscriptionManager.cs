using System.Collections.Concurrent;
using System.IO.Pipes;

namespace MCCS.Infrastructure.Communication.NamedPipe.PubSub;

/// <summary>
/// 订阅管理器实现 - 线程安全的主题订阅管理
/// </summary>
public sealed class SubscriptionManager : ISubscriptionManager
{
    // 主题 -> 订阅者列表
    private readonly ConcurrentDictionary<string, ConcurrentDictionary<string, SubscriptionInfo>> _topicSubscribers = new(StringComparer.OrdinalIgnoreCase);

    // 订阅者 -> 主题列表
    private readonly ConcurrentDictionary<string, ConcurrentDictionary<string, SubscriptionInfo>> _subscriberTopics = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// 订阅添加事件
    /// </summary>
    public event Action<string, string>? SubscriptionAdded;

    /// <summary>
    /// 订阅移除事件
    /// </summary>
    public event Action<string, string>? SubscriptionRemoved;

    public SubscriptionInfo Subscribe(string subscriberId, string topic, NamedPipeServerStream pipeStream)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(subscriberId);
        ArgumentException.ThrowIfNullOrWhiteSpace(topic);
        ArgumentNullException.ThrowIfNull(pipeStream);

        var subscription = new SubscriptionInfo(subscriberId, topic)
        {
            PipeStream = pipeStream
        };

        // 添加到主题->订阅者映射
        var topicSubs = _topicSubscribers.GetOrAdd(topic, _ => new ConcurrentDictionary<string, SubscriptionInfo>(StringComparer.OrdinalIgnoreCase));
        topicSubs.AddOrUpdate(subscriberId, subscription, (_, existing) =>
        {
            existing.WriteLock.Dispose();
            return subscription;
        });

        // 添加到订阅者->主题映射
        var subscriberSubs = _subscriberTopics.GetOrAdd(subscriberId, _ => new ConcurrentDictionary<string, SubscriptionInfo>(StringComparer.OrdinalIgnoreCase));
        subscriberSubs.AddOrUpdate(topic, subscription, (_, _) => subscription);

        SubscriptionAdded?.Invoke(subscriberId, topic);

        return subscription;
    }

    public bool Unsubscribe(string subscriberId, string? topic = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(subscriberId);

        if (string.IsNullOrEmpty(topic))
        {
            // 取消所有订阅
            RemoveSubscriber(subscriberId);
            return true;
        }

        // 从主题->订阅者映射中移除
        if (_topicSubscribers.TryGetValue(topic, out var topicSubs))
        {
            if (topicSubs.TryRemove(subscriberId, out var removed))
            {
                removed.WriteLock.Dispose();
            }

            // 如果主题没有订阅者了，移除主题
            if (topicSubs.IsEmpty)
            {
                _topicSubscribers.TryRemove(topic, out _);
            }
        }

        // 从订阅者->主题映射中移除
        if (_subscriberTopics.TryGetValue(subscriberId, out var subscriberSubs))
        {
            subscriberSubs.TryRemove(topic, out _);

            // 如果订阅者没有订阅了，移除订阅者
            if (subscriberSubs.IsEmpty)
            {
                _subscriberTopics.TryRemove(subscriberId, out _);
            }
        }

        SubscriptionRemoved?.Invoke(subscriberId, topic);

        return true;
    }

    public void RemoveSubscriber(string subscriberId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(subscriberId);

        // 获取该订阅者的所有订阅
        if (!_subscriberTopics.TryRemove(subscriberId, out var subscriberSubs))
        {
            return;
        }

        // 从每个主题中移除该订阅者
        foreach (var (topic, subscription) in subscriberSubs)
        {
            if (_topicSubscribers.TryGetValue(topic, out var topicSubs))
            {
                topicSubs.TryRemove(subscriberId, out _);

                // 如果主题没有订阅者了，移除主题
                if (topicSubs.IsEmpty)
                {
                    _topicSubscribers.TryRemove(topic, out _);
                }
            }

            subscription.WriteLock.Dispose();
            SubscriptionRemoved?.Invoke(subscriberId, topic);
        }
    }

    public IReadOnlyList<SubscriptionInfo> GetSubscribers(string topic)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(topic);

        if (_topicSubscribers.TryGetValue(topic, out var topicSubs))
        {
            return topicSubs.Values.ToList();
        }

        return [];
    }

    public IReadOnlyList<string> GetSubscriptions(string subscriberId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(subscriberId);

        if (_subscriberTopics.TryGetValue(subscriberId, out var subscriberSubs))
        {
            return subscriberSubs.Keys.ToList();
        }

        return [];
    }

    public bool IsSubscribed(string subscriberId, string topic)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(subscriberId);
        ArgumentException.ThrowIfNullOrWhiteSpace(topic);

        return _subscriberTopics.TryGetValue(subscriberId, out var subscriberSubs)
               && subscriberSubs.ContainsKey(topic);
    }

    public IReadOnlyList<string> GetAllTopics()
    {
        return _topicSubscribers.Keys.ToList();
    }

    public int GetSubscriberCount(string topic)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(topic);

        if (_topicSubscribers.TryGetValue(topic, out var topicSubs))
        {
            return topicSubs.Count;
        }

        return 0;
    }

    public void Clear()
    {
        foreach (var (_, topicSubs) in _topicSubscribers)
        {
            foreach (var (_, subscription) in topicSubs)
            {
                subscription.WriteLock.Dispose();
            }
        }

        _topicSubscribers.Clear();
        _subscriberTopics.Clear();
    }
}
