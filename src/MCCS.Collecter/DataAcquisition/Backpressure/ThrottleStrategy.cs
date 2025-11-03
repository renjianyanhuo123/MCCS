using System.Reactive.Linq;

namespace MCCS.Collecter.DataAcquisition.Backpressure;

/// <summary>
/// 节流策略
/// 仅在指定的时间间隔内没有新数据时发出最后一个值
/// </summary>
public class ThrottleStrategy : IBackpressureStrategy
{
    private readonly TimeSpan _throttleDuration;

    public string Name => "Throttle";

    public ThrottleStrategy(TimeSpan throttleDuration)
    {
        if (throttleDuration <= TimeSpan.Zero)
            throw new ArgumentException("Throttle duration must be positive", nameof(throttleDuration));

        _throttleDuration = throttleDuration;
    }

    public IObservable<T> Apply<T>(IObservable<T> source)
    {
        // Rx.NET 的 Throttle 操作符：只发出在静默期后的值
        return source.Throttle(_throttleDuration);
    }
}
