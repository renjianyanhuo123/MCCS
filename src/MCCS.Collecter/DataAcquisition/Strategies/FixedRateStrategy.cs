using System.Diagnostics;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using MCCS.Collecter.HardwareAdapters;
using MCCS.Infrastructure.TestModels.DataAcquisition;

namespace MCCS.Collecter.DataAcquisition.Strategies;

/// <summary>
/// 固定频率采集策略
/// </summary>
public class FixedRateStrategy : IDataAcquisitionStrategy
{
    public string Name => "FixedRate";
    public int SampleRate { get; }

    public FixedRateStrategy(int sampleRate)
    {
        if (sampleRate <= 0)
            throw new ArgumentException("Sample rate must be positive", nameof(sampleRate));

        SampleRate = sampleRate;
    }

    public IObservable<RawHardwareData> CreateAcquisitionStream(
        IHardwareAdapter adapter,
        IScheduler scheduler,
        CancellationToken cancellationToken)
    {
        return Observable
            .Generate(
                0L, // 初始状态
                _ => !cancellationToken.IsCancellationRequested, // 继续条件
                tick => tick + 1, // 状态更新
                _ => adapter.ReadData(), // 读取数据
                _ => CalculateInterval()) // 时间间隔
            .ObserveOn(scheduler);
    }

    private TimeSpan CalculateInterval()
    {
        // 精确计算采样间隔
        return TimeSpan.FromTicks(Stopwatch.Frequency / SampleRate);
    }
}
