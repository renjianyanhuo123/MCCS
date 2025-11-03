using System.Collections.Concurrent;
using System.Reactive.Linq;

namespace MCCS.Collecter.DataAcquisition.Backpressure;

/// <summary>
/// 丢弃最旧策略
/// 当缓冲区满时，丢弃最旧的数据
/// </summary>
public class DropOldestStrategy : IBackpressureStrategy
{
    private readonly int _bufferSize;

    public string Name => "DropOldest";

    public DropOldestStrategy(int bufferSize)
    {
        if (bufferSize <= 0)
            throw new ArgumentException("Buffer size must be positive", nameof(bufferSize));

        _bufferSize = bufferSize;
    }

    public IObservable<T> Apply<T>(IObservable<T> source)
    {
        return Observable.Create<T>(observer =>
        {
            var buffer = new ConcurrentQueue<T>();

            return source.Subscribe(
                item =>
                {
                    // 如果缓冲区满，移除最旧的
                    if (buffer.Count >= _bufferSize)
                    {
                        buffer.TryDequeue(out _);
                    }

                    buffer.Enqueue(item);
                    observer.OnNext(item);
                },
                observer.OnError,
                observer.OnCompleted
            );
        });
    }
}
