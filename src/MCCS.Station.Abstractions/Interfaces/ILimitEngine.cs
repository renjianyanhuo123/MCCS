using MCCS.Station.Abstractions.Enums;
using MCCS.Station.Abstractions.Events;
using MCCS.Station.Abstractions.Models;

namespace MCCS.Station.Abstractions.Interfaces;

/// <summary>
/// 软限位引擎接口
/// 负责过程保护，保护试样/过程，系统仍可控
/// </summary>
public interface ILimitEngine
{
    /// <summary>
    /// 软限位触发事件流
    /// </summary>
    IObservable<SoftLimitTriggeredEvent> LimitTriggered { get; }

    /// <summary>
    /// 限位警告事件流（接近限位时预警）
    /// </summary>
    IObservable<LimitTrippedEvent> LimitWarning { get; }

    /// <summary>
    /// 注册软限位配置
    /// </summary>
    void RegisterLimit(SoftLimitConfig config);

    /// <summary>
    /// 批量注册软限位配置
    /// </summary>
    void RegisterLimits(IEnumerable<SoftLimitConfig> configs);

    /// <summary>
    /// 移除软限位配置
    /// </summary>
    bool RemoveLimit(string limitId);

    /// <summary>
    /// 清除所有限位配置
    /// </summary>
    void ClearAllLimits();

    /// <summary>
    /// 获取所有限位配置
    /// </summary>
    IReadOnlyList<SoftLimitConfig> GetAllLimits();

    /// <summary>
    /// 获取指定通道的限位配置
    /// </summary>
    IReadOnlyList<SoftLimitConfig> GetLimitsForChannel(string channelId);

    /// <summary>
    /// 启用/禁用指定限位
    /// </summary>
    void SetLimitEnabled(string limitId, bool enabled);

    /// <summary>
    /// 更新限位阈值
    /// </summary>
    void UpdateLimitThreshold(string limitId, double? upperLimit = null, double? lowerLimit = null);

    /// <summary>
    /// 评估信号值（检查是否触发限位）
    /// </summary>
    SoftLimitState? EvaluateSignal(string channelId, string signalName, double value);

    /// <summary>
    /// 批量评估信号
    /// </summary>
    IReadOnlyList<SoftLimitState> EvaluateSignals(IEnumerable<(string ChannelId, string SignalName, double Value)> signals);

    /// <summary>
    /// 获取当前所有限位状态
    /// </summary>
    IReadOnlyList<SoftLimitState> GetAllLimitStates();

    /// <summary>
    /// 获取所有触发的限位
    /// </summary>
    IReadOnlyList<SoftLimitState> GetTrippedLimits();

    /// <summary>
    /// 获取所有警告状态的限位
    /// </summary>
    IReadOnlyList<SoftLimitState> GetWarningLimits();

    /// <summary>
    /// 手动确认限位（用于解除需要确认的限位）
    /// </summary>
    bool AcknowledgeLimit(string limitId, string operatorId, string reason);

    /// <summary>
    /// 检查是否有任何限位触发
    /// </summary>
    bool HasAnyTrippedLimits { get; }

    /// <summary>
    /// 检查是否有任何限位警告
    /// </summary>
    bool HasAnyWarnings { get; }

    /// <summary>
    /// 获取当前限位导致的禁用能力
    /// </summary>
    CapabilityFlags GetDisabledCapabilities();
}
