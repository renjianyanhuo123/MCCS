using MCCS.Station.Abstractions.Enums;
using MCCS.Station.Abstractions.Events;
using MCCS.Station.Abstractions.Models;

namespace MCCS.Station.Abstractions.Interfaces;

/// <summary>
/// 安全主管接口
/// 整合 LimitEngine、InterlockEngine、EStopMonitor
/// 独立于状态机，对状态机有"硬打断/降级/锁定"的权限
/// </summary>
public interface ISafetySupervisor
{
    /// <summary>
    /// 当前安全状态
    /// </summary>
    SafetyStatus CurrentSafetyStatus { get; }

    /// <summary>
    /// 安全状态变更事件流
    /// </summary>
    IObservable<SafetyStatusChangedEvent> SafetyStatusChanged { get; }

    /// <summary>
    /// 软限位引擎
    /// </summary>
    ILimitEngine LimitEngine { get; }

    /// <summary>
    /// 联锁引擎
    /// </summary>
    IInterlockEngine InterlockEngine { get; }

    /// <summary>
    /// 急停监控器
    /// </summary>
    IEStopMonitor EStopMonitor { get; }

    /// <summary>
    /// 获取当前被禁用的能力
    /// </summary>
    CapabilityFlags GetDisabledCapabilities();

    /// <summary>
    /// 获取所有活跃的安全问题
    /// </summary>
    IReadOnlyList<StatusIssue> GetActiveSecurityIssues();

    /// <summary>
    /// 获取所有触发的联锁状态
    /// </summary>
    IReadOnlyList<InterlockState> GetTrippedInterlocks();

    /// <summary>
    /// 获取所有触发的软限位状态
    /// </summary>
    IReadOnlyList<SoftLimitState> GetTrippedLimits();

    /// <summary>
    /// 尝试清除指定联锁
    /// </summary>
    Task<bool> TryClearInterlockAsync(string ruleId, string operatorId, string reason);

    /// <summary>
    /// 尝试复位急停
    /// </summary>
    Task<bool> TryResetEStopAsync(string operatorId, string reason);

    /// <summary>
    /// 触发软件急停
    /// </summary>
    Task TriggerSoftwareEStopAsync(string reason, string triggeredBy);

    /// <summary>
    /// 释放软件急停
    /// </summary>
    Task<bool> TryReleaseSoftwareEStopAsync(string operatorId, string reason);

    /// <summary>
    /// 检查是否允许执行指定操作
    /// </summary>
    (bool Allowed, string Reason, IReadOnlyList<string> BlockingRules) CheckOperation(CapabilityFlags requiredCapabilities);

    /// <summary>
    /// 启动安全监控
    /// </summary>
    Task StartAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// 停止安全监控
    /// </summary>
    Task StopAsync();

    /// <summary>
    /// 是否正在运行
    /// </summary>
    bool IsRunning { get; }
}
