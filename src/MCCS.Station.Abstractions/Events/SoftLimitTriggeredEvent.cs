using MCCS.Station.Abstractions.Enums;
using MCCS.Station.Abstractions.Models;

namespace MCCS.Station.Abstractions.Events;

/// <summary>
/// 软限位触发事件
/// 当软限位被触发或解除时发出
/// </summary>
public record SoftLimitTriggeredEvent : StationEvent
{
    /// <summary>
    /// 限位配置ID
    /// </summary>
    public string LimitId { get; init; } = string.Empty;

    /// <summary>
    /// 通道ID
    /// </summary>
    public string ChannelId { get; init; } = string.Empty;

    /// <summary>
    /// 信号名称
    /// </summary>
    public string SignalName { get; init; } = string.Empty;

    /// <summary>
    /// 限位类型
    /// </summary>
    public SoftLimitType LimitType { get; init; }

    /// <summary>
    /// 是否触发（false表示解除）
    /// </summary>
    public bool IsTripped { get; init; }

    /// <summary>
    /// 是否是上限触发
    /// </summary>
    public bool IsUpperLimit { get; init; }

    /// <summary>
    /// 限位阈值
    /// </summary>
    public double Threshold { get; init; }

    /// <summary>
    /// 触发时的实际值
    /// </summary>
    public double ActualValue { get; init; }

    /// <summary>
    /// 超出量
    /// </summary>
    public double Overshoot { get; init; }

    /// <summary>
    /// 执行的动作
    /// </summary>
    public LimitAction ActionTaken { get; init; }

    /// <summary>
    /// 单位
    /// </summary>
    public string Unit { get; init; } = string.Empty;

    /// <summary>
    /// 关联的段ID（如果在试验中）
    /// </summary>
    public Guid? SegmentId { get; init; }
}
