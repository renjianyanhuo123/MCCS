using System.Reactive.Linq;

namespace MCCS.Collecter.DataAcquisition.Backpressure;

/// <summary>
/// 批处理策略
/// 将多个数据项批处理后再发出，减少事件频率
/// </summary>
public class BatchingStrategy : IBackpressureStrategy
{
    private readonly int _batchSize;

    public string Name => "Batching";

    public BatchingStrategy(int batchSize)
    {
        if (batchSize <= 0)
            throw new ArgumentException("Batch size must be positive", nameof(batchSize));

        _batchSize = batchSize;
    }

    public IObservable<T> Apply<T>(IObservable<T> source)
    {
        // 缓冲指定数量的数据，然后展平发出
        return source
            .Buffer(_batchSize)
            .SelectMany(batch => batch);
    }
}
