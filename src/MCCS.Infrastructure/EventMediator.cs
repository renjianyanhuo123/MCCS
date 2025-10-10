using System;

namespace MCCS.Infrastructure
{
    /// <summary>
    /// 事件中介者 - 发布订阅模式
    /// </summary>
    public class EventMediator
    {
        private static readonly Lazy<EventMediator> _instance = new(() => new EventMediator());

        public static EventMediator Instance => _instance.Value;

        private readonly Dictionary<Type, List<WeakReference>> _subscribers = new();

        private readonly object _lock = new();

        private EventMediator() { }

        /// <summary>
        /// 订阅事件
        /// </summary>
        public void Subscribe<T>(EventHandler<T> handler) where T : class
        {
            lock (_lock)
            {
                var messageType = typeof(T);

                if (!_subscribers.ContainsKey(messageType))
                {
                    _subscribers[messageType] = [];
                }

                _subscribers[messageType].Add(new WeakReference(handler));
            }
        }

        /// <summary>
        /// 取消订阅
        /// </summary>
        public void Unsubscribe<T>(EventHandler<T> handler) where T : class
        {
            lock (_lock)
            {
                var messageType = typeof(T);

                if (_subscribers.TryGetValue(messageType, out var subscriber))
                {
                    subscriber.RemoveAll(wr =>
                    {
                        var target = wr.Target as EventHandler<T>;
                        return target == null || target == handler;
                    });
                }
            }
        }

        /// <summary>
        /// 发布事件
        /// </summary>
        public void Publish<T>(T message) where T : class
        {
            List<WeakReference> subscribers;

            lock (_lock)
            {
                var messageType = typeof(T);

                if (!_subscribers.TryGetValue(messageType, out var subscriber))
                    return;

                subscribers = subscriber.ToList();

                // 清理已回收的订阅者
                _subscribers[messageType].RemoveAll(wr => !wr.IsAlive);
            }

            // 在锁外执行回调，避免死锁
            foreach (var weakRef in subscribers)
            {
                if (weakRef.Target is EventHandler<T> handler)
                {
                    try
                    {
                        handler.Invoke(null, message);
                    }
                    catch (Exception ex)
                    { 
                    }
                }
            }
        }

        /// <summary>
        /// 清空所有订阅
        /// </summary>
        public void Clear()
        {
            lock (_lock)
            {
                _subscribers.Clear();
            }
        }
    }
}
