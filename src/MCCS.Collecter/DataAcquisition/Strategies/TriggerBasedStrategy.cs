using System.Reactive.Concurrency;
using System.Reactive.Linq;
using MCCS.Collecter.HardwareAdapters;
using MCCS.Infrastructure.TestModels.DataAcquisition;

namespace MCCS.Collecter.DataAcquisition.Strategies;

/// <summary>
/// 触发事件
/// </summary>
public record TriggerEvent
{
    public long Timestamp { get; init; }
    public string? Source { get; init; }
    public Dictionary<string, object>? Data { get; init; }
}

/// <summary>
/// 触发式采集策略（事件驱动）
/// </summary>
public class TriggerBasedStrategy : IDataAcquisitionStrategy
{
    private readonly IObservable<TriggerEvent> _triggerSource;

    public string Name => "TriggerBased";
    public int SampleRate => 0; // 非周期性

    public TriggerBasedStrategy(IObservable<TriggerEvent> triggerSource)
    {
        _triggerSource = triggerSource ?? throw new ArgumentNullException(nameof(triggerSource));
    }

    public IObservable<RawHardwareData> CreateAcquisitionStream(
        IHardwareAdapter adapter,
        IScheduler scheduler,
        CancellationToken cancellationToken)
    {
        return _triggerSource
            .TakeWhile(_ => !cancellationToken.IsCancellationRequested)
            .Select(_ => adapter.ReadData())
            .ObserveOn(scheduler);
    }
}
