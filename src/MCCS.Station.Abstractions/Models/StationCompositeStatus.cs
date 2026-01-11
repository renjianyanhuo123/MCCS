using MCCS.Station.Abstractions.Enums;

namespace MCCS.Station.Abstractions.Models;

/// <summary>
/// 站点复合状态
/// 站点"总状态"不是一个枚举，而是四个正交维度的组合
/// UI上可以一眼看出"连着但联锁了""运行中但能量已降级""硬件齐全但处于低压不可试验"等真实情况
/// </summary>
public sealed class StationCompositeStatus
{
    /// <summary>
    /// 连接与资源维度状态
    /// </summary>
    public ConnectivityStatus Connectivity { get; init; } = ConnectivityStatus.Disconnected;

    /// <summary>
    /// 激活与能量维度状态
    /// </summary>
    public ActivationStatus Activation { get; init; } = ActivationStatus.Off;

    /// <summary>
    /// 运行流程维度状态
    /// </summary>
    public ProcessStatus Process { get; init; } = ProcessStatus.Idle;

    /// <summary>
    /// 安全与保护维度状态
    /// </summary>
    public SafetyStatus Safety { get; init; } = SafetyStatus.Normal;

    /// <summary>
    /// 当前允许的操作能力（由各维度综合计算）
    /// </summary>
    public CapabilityFlags Capabilities { get; init; } = CapabilityFlags.None;

    /// <summary>
    /// 状态摘要（用于快速显示）
    /// </summary>
    public string Summary { get; init; } = string.Empty;

    /// <summary>
    /// 活跃的警告/问题列表
    /// </summary>
    public IReadOnlyList<StatusIssue> ActiveIssues { get; init; } = [];

    /// <summary>
    /// 状态更新时间
    /// </summary>
    public DateTime UpdatedAt { get; init; } = DateTime.UtcNow;

    /// <summary>
    /// 是否可以进行控制操作
    /// </summary>
    public bool CanOperate =>
        Connectivity == ConnectivityStatus.Ready &&
        Activation >= ActivationStatus.High &&
        Safety <= SafetyStatus.Warning;

    /// <summary>
    /// 是否可以启动试验
    /// </summary>
    public bool CanStartTest =>
        CanOperate &&
        Process is ProcessStatus.Idle or ProcessStatus.Armed or ProcessStatus.Completed &&
        Safety == SafetyStatus.Normal;

    /// <summary>
    /// 是否正在运行试验
    /// </summary>
    public bool IsRunning => Process == ProcessStatus.Running;

    /// <summary>
    /// 是否处于安全受限状态
    /// </summary>
    public bool IsSafetyRestricted => Safety >= SafetyStatus.Limited;

    /// <summary>
    /// 是否需要人工干预
    /// </summary>
    public bool RequiresIntervention => Safety >= SafetyStatus.Interlocked; 
    public override string ToString() => $"[{Connectivity}|{Activation}|{Process}|{Safety}] {Summary}";
}

/// <summary>
/// 状态问题/警告项
/// </summary>
public sealed class StatusIssue
{
    /// <summary>
    /// 问题ID
    /// </summary>
    public string Id { get; init; } = string.Empty;

    /// <summary>
    /// 问题来源（资源ID或模块名）
    /// </summary>
    public string Source { get; init; } = string.Empty;

    /// <summary>
    /// 问题级别
    /// </summary>
    public SafetyStatus Level { get; init; } = SafetyStatus.Warning;

    /// <summary>
    /// 问题描述
    /// </summary>
    public string Message { get; init; } = string.Empty;

    /// <summary>
    /// 发生时间
    /// </summary>
    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;

    /// <summary>
    /// 是否可自动恢复
    /// </summary>
    public bool AutoRecoverable { get; init; }

    /// <summary>
    /// 恢复操作提示
    /// </summary>
    public string RecoveryHint { get; init; } = string.Empty;
}
