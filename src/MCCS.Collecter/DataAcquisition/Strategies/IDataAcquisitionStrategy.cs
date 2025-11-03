using System.Reactive.Concurrency;
using MCCS.Collecter.HardwareAdapters;
using MCCS.Infrastructure.TestModels.DataAcquisition;

namespace MCCS.Collecter.DataAcquisition.Strategies;

/// <summary>
/// 数据采集策略接口
/// </summary>
public interface IDataAcquisitionStrategy
{
    /// <summary>
    /// 策略名称
    /// </summary>
    string Name { get; }

    /// <summary>
    /// 采样率 (Hz)，0 表示非周期性
    /// </summary>
    int SampleRate { get; }

    /// <summary>
    /// 创建采集的 Observable 流
    /// </summary>
    /// <param name="adapter">硬件适配器</param>
    /// <param name="scheduler">调度器</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>原始硬件数据流</returns>
    IObservable<RawHardwareData> CreateAcquisitionStream(
        IHardwareAdapter adapter,
        IScheduler scheduler,
        CancellationToken cancellationToken
    );
}
