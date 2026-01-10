using MCCS.Station.Abstractions.Enums;
using MCCS.Station.Abstractions.Models;

namespace MCCS.Station.Abstractions.Events;

/// <summary>
/// 联锁触发事件
/// 当联锁被触发或清除时发出
/// </summary>
public record InterlockTriggeredEvent : StationEvent
{
    /// <summary>
    /// 联锁规则ID
    /// </summary>
    public string RuleId { get; init; } = string.Empty;

    /// <summary>
    /// 联锁类型
    /// </summary>
    public InterlockTypeEnum InterlockType { get; init; }

    /// <summary>
    /// 是否触发（false表示清除）
    /// </summary>
    public bool IsTripped { get; init; }

    /// <summary>
    /// 是否锁存
    /// </summary>
    public bool IsLatched { get; init; }

    /// <summary>
    /// 触发原因
    /// </summary>
    public string Reason { get; init; } = string.Empty;

    /// <summary>
    /// 触发时的信号值
    /// </summary>
    public IReadOnlyDictionary<string, double>? TriggerValues { get; init; }

    /// <summary>
    /// 执行的动作
    /// </summary>
    public InterlockAction ActionTaken { get; init; }

    /// <summary>
    /// 受影响的能力
    /// </summary>
    public CapabilityFlags DisabledCapabilities { get; init; }

    /// <summary>
    /// 复位策略
    /// </summary>
    public InterlockResetPolicy ResetPolicy { get; init; }

    /// <summary>
    /// 清除指令
    /// </summary>
    public string ClearInstructions { get; init; } = string.Empty;
}
