using MCCS.Station.Abstractions.Enums;
using MCCS.Station.Abstractions.Models;

namespace MCCS.Station.Abstractions.Events;

/// <summary>
/// 复合状态变更事件
/// 当站点的任何维度状态发生变化时触发
/// </summary>
public record CompositeStatusChangedEvent : StationEvent
{
    /// <summary>
    /// 之前的状态
    /// </summary>
    public StationCompositeStatus PreviousStatus { get; init; } = null!;

    /// <summary>
    /// 当前状态
    /// </summary>
    public StationCompositeStatus CurrentStatus { get; init; } = null!;

    /// <summary>
    /// 变化的维度
    /// </summary>
    public StatusDimension ChangedDimension { get; init; }

    /// <summary>
    /// 变化原因
    /// </summary>
    public string Reason { get; init; } = string.Empty;

    /// <summary>
    /// 是否由安全系统触发
    /// </summary>
    public bool TriggeredBySafety { get; init; }
}

/// <summary>
/// 状态维度
/// </summary>
[Flags]
public enum StatusDimension : byte
{
    None = 0,
    Connectivity = 1 << 0,
    Activation = 1 << 1,
    Process = 1 << 2,
    Safety = 1 << 3,
    Capabilities = 1 << 4,
    All = Connectivity | Activation | Process | Safety | Capabilities
}
