using MCCS.Station.Abstractions.Enums;
using MCCS.Station.Abstractions.Events;
using MCCS.Station.Abstractions.Models;

namespace MCCS.Station.Abstractions.Interfaces;

/// <summary>
/// 状态合成器接口
/// 负责将四个正交维度合成为站点总状态
/// 按优先级从高到低覆盖：EStop > Interlocked > Limited > Degraded > 流程状态
/// </summary>
public interface IStatusAggregator
{
    /// <summary>
    /// 获取当前复合状态
    /// </summary>
    StationCompositeStatus CurrentStatus { get; }

    /// <summary>
    /// 复合状态变更事件流
    /// </summary>
    IObservable<CompositeStatusChangedEvent> StatusChanged { get; }

    /// <summary>
    /// 更新连接维度状态
    /// </summary>
    void UpdateConnectivity(ConnectivityStatus status, string reason = "");

    /// <summary>
    /// 更新激活维度状态
    /// </summary>
    void UpdateActivation(ActivationStatus status, string reason = "");

    /// <summary>
    /// 更新流程维度状态
    /// </summary>
    void UpdateProcess(ProcessStatus status, string reason = "");

    /// <summary>
    /// 更新安全维度状态（通常由SafetySupervisor调用）
    /// </summary>
    void UpdateSafety(SafetyStatus status, string reason = "", bool triggeredBySafety = true);

    /// <summary>
    /// 添加活跃问题
    /// </summary>
    void AddIssue(StatusIssue issue);

    /// <summary>
    /// 移除活跃问题
    /// </summary>
    void RemoveIssue(string issueId);

    /// <summary>
    /// 清除所有可自动恢复的问题
    /// </summary>
    void ClearAutoRecoverableIssues();

    /// <summary>
    /// 重新计算能力
    /// </summary>
    CapabilityFlags RecalculateCapabilities();

    /// <summary>
    /// 检查是否具有指定能力
    /// </summary>
    bool HasCapability(CapabilityFlags capability);

    /// <summary>
    /// 检查是否可以执行指定命令类型
    /// </summary>
    bool CanExecuteCommand(CommandType commandType);

    /// <summary>
    /// 获取状态摘要文本
    /// </summary>
    string GetStatusSummary();

    /// <summary>
    /// 获取所有活跃问题
    /// </summary>
    IReadOnlyList<StatusIssue> GetActiveIssues();

    /// <summary>
    /// 强制刷新状态（从各子系统重新采集）
    /// </summary>
    Task RefreshAsync(CancellationToken cancellationToken = default);
}
