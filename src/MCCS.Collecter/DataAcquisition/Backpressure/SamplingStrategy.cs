using System.Reactive.Linq;

namespace MCCS.Collecter.DataAcquisition.Backpressure;

/// <summary>
/// 采样策略
/// 降低数据率，定期采样
/// </summary>
public class SamplingStrategy : IBackpressureStrategy
{
    private readonly TimeSpan _sampleInterval;

    public string Name => "Sampling";

    public SamplingStrategy(TimeSpan sampleInterval)
    {
        if (sampleInterval <= TimeSpan.Zero)
            throw new ArgumentException("Sample interval must be positive", nameof(sampleInterval));

        _sampleInterval = sampleInterval;
    }

    public IObservable<T> Apply<T>(IObservable<T> source)
    {
        // Rx.NET 的 Sample 操作符：在每个时间间隔内采样最后一个值
        return source.Sample(_sampleInterval);
    }
}
