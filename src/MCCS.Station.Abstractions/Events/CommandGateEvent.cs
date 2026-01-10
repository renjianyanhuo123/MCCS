using MCCS.Station.Abstractions.Enums;
using MCCS.Station.Abstractions.Models;

namespace MCCS.Station.Abstractions.Events;

/// <summary>
/// 命令过闸事件
/// 记录命令通过或被拒绝
/// </summary>
public record CommandGateEvent : StationEvent
{
    /// <summary>
    /// 命令ID
    /// </summary>
    public Guid CommandId { get; init; }

    /// <summary>
    /// 命令类型
    /// </summary>
    public CommandType CommandType { get; init; }

    /// <summary>
    /// 命令来源
    /// </summary>
    public string CommandSource { get; init; } = string.Empty;

    /// <summary>
    /// 目标通道
    /// </summary>
    public string? TargetChannel { get; init; }

    /// <summary>
    /// 是否通过
    /// </summary>
    public bool Passed { get; init; }

    /// <summary>
    /// 拒绝原因（如果被拒绝）
    /// </summary>
    public CommandRejectionReason? RejectionReason { get; init; }

    /// <summary>
    /// 拒绝详情
    /// </summary>
    public string RejectionDetails { get; init; } = string.Empty;

    /// <summary>
    /// 阻止的规则
    /// </summary>
    public IReadOnlyList<string>? BlockingRules { get; init; }

    /// <summary>
    /// 缺失的能力
    /// </summary>
    public CapabilityFlags? MissingCapabilities { get; init; }

    /// <summary>
    /// 当时的安全状态
    /// </summary>
    public SafetyStatus SafetyStatus { get; init; }
}
