using MCCS.Station.Abstractions.Enums;

namespace MCCS.Station.Abstractions.Models;

/// <summary>
/// 联锁规则定义
/// 联锁目的：防止危险动作发生，强调"条件不满足就禁止"
/// </summary>
public sealed class InterlockRule
{
    /// <summary>
    /// 联锁规则ID
    /// </summary>
    public string RuleId { get; init; } = string.Empty;

    /// <summary>
    /// 联锁名称
    /// </summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// 联锁类型
    /// </summary>
    public InterlockTypeEnum Type { get; init; }

    /// <summary>
    /// 联锁描述
    /// </summary>
    public string Description { get; init; } = string.Empty;

    /// <summary>
    /// 关联的信号/条件源
    /// </summary>
    public IReadOnlyList<string> SourceSignals { get; init; } = Array.Empty<string>();

    /// <summary>
    /// 联锁条件表达式（用于评估是否触发）
    /// </summary>
    public string ConditionExpression { get; init; } = string.Empty;

    /// <summary>
    /// 是否锁存（触发后需要手动复位）
    /// </summary>
    public bool IsLatched { get; init; } = true;

    /// <summary>
    /// 复位策略
    /// </summary>
    public InterlockResetPolicy ResetPolicy { get; init; } = InterlockResetPolicy.Manual;

    /// <summary>
    /// 触发时禁止的能力
    /// </summary>
    public CapabilityFlags DisabledCapabilities { get; init; } = CapabilityFlags.Full;

    /// <summary>
    /// 触发时应执行的动作
    /// </summary>
    public InterlockAction TriggerAction { get; init; } = InterlockAction.Hold;

    /// <summary>
    /// 优先级（数值越大优先级越高）
    /// </summary>
    public int Priority { get; init; } = 100;

    /// <summary>
    /// 是否启用
    /// </summary>
    public bool IsEnabled { get; init; } = true;

    /// <summary>
    /// 附加元数据
    /// </summary>
    public IReadOnlyDictionary<string, string>? Metadata { get; init; }
}

/// <summary>
/// 联锁触发时执行的动作
/// </summary>
public enum InterlockAction : byte
{
    /// <summary>
    /// 保持当前状态
    /// </summary>
    Hold = 0,

    /// <summary>
    /// 停止运动
    /// </summary>
    Stop = 1,

    /// <summary>
    /// 软停止（减速停止）
    /// </summary>
    SoftStop = 2,

    /// <summary>
    /// 关闭阀台
    /// </summary>
    CloseValve = 3,

    /// <summary>
    /// 切断能量
    /// </summary>
    CutPower = 4,

    /// <summary>
    /// 回到安全位置
    /// </summary>
    ReturnToSafe = 5
}

/// <summary>
/// 联锁状态（运行时）
/// </summary>
public sealed class InterlockState
{
    /// <summary>
    /// 关联的规则
    /// </summary>
    public InterlockRule Rule { get; init; } = null!;

    /// <summary>
    /// 是否已触发
    /// </summary>
    public bool IsTripped { get; init; }

    /// <summary>
    /// 是否已锁存
    /// </summary>
    public bool IsLatched { get; init; }

    /// <summary>
    /// 触发时间
    /// </summary>
    public DateTime? TrippedAt { get; init; }

    /// <summary>
    /// 触发原因详情
    /// </summary>
    public string TripReason { get; init; } = string.Empty;

    /// <summary>
    /// 触发时的信号值
    /// </summary>
    public IReadOnlyDictionary<string, double>? TriggerValues { get; init; }

    /// <summary>
    /// 清除联锁需要的操作
    /// </summary>
    public string ClearInstructions { get; init; } = string.Empty;

    /// <summary>
    /// 可以尝试清除
    /// </summary>
    public bool CanAttemptClear => IsTripped && (!IsLatched || Rule.ResetPolicy != InterlockResetPolicy.Hardware);
}
