using MCCS.Station.Abstractions.Enums;

namespace MCCS.Station.Abstractions.Events;

/// <summary>
/// 安全状态变更事件
/// 安全系统状态发生变化时触发
/// </summary>
public record SafetyStatusChangedEvent : StationEvent
{
    /// <summary>
    /// 之前的安全状态
    /// </summary>
    public SafetyStatus PreviousStatus { get; init; }

    /// <summary>
    /// 当前安全状态
    /// </summary>
    public SafetyStatus CurrentStatus { get; init; }

    /// <summary>
    /// 触发原因
    /// </summary>
    public SafetyTriggerReason TriggerReason { get; init; }

    /// <summary>
    /// 触发的规则ID列表
    /// </summary>
    public IReadOnlyList<string> TriggeredRules { get; init; } = Array.Empty<string>();

    /// <summary>
    /// 详细描述
    /// </summary>
    public string Description { get; init; } = string.Empty;

    /// <summary>
    /// 受影响的通道
    /// </summary>
    public IReadOnlyList<string> AffectedChannels { get; init; } = Array.Empty<string>();

    /// <summary>
    /// 执行的动作
    /// </summary>
    public string ActionTaken { get; init; } = string.Empty;

    /// <summary>
    /// 是否需要人工干预
    /// </summary>
    public bool RequiresIntervention { get; init; }
}

/// <summary>
/// 安全触发原因
/// </summary>
public enum SafetyTriggerReason : byte
{
    /// <summary>
    /// 无
    /// </summary>
    None = 0,

    /// <summary>
    /// 软限位触发
    /// </summary>
    SoftLimitTripped = 1,

    /// <summary>
    /// 联锁触发
    /// </summary>
    InterlockTripped = 2,

    /// <summary>
    /// 急停触发
    /// </summary>
    EStopTripped = 3,

    /// <summary>
    /// 失控保护触发
    /// </summary>
    FailsafeTripped = 4,

    /// <summary>
    /// 硬件故障
    /// </summary>
    HardwareFault = 5,

    /// <summary>
    /// 通信丢失
    /// </summary>
    CommunicationLost = 6,

    /// <summary>
    /// 手动触发
    /// </summary>
    ManualTrigger = 7,

    /// <summary>
    /// 条件恢复（安全状态改善）
    /// </summary>
    ConditionRestored = 10,

    /// <summary>
    /// 手动清除
    /// </summary>
    ManualClear = 11,

    /// <summary>
    /// 复位操作
    /// </summary>
    Reset = 12
}
