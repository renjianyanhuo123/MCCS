using MCCS.Station.Abstractions.Enums;
using MCCS.Station.Abstractions.Events;
using MCCS.Station.Abstractions.Models;

namespace MCCS.Station.Abstractions.Interfaces;

/// <summary>
/// 联锁引擎接口
/// 负责防止危险动作发生，强调"条件不满足就禁止"
/// </summary>
public interface IInterlockEngine
{
    /// <summary>
    /// 联锁触发事件流
    /// </summary>
    IObservable<InterlockTriggeredEvent> InterlockTriggered { get; }

    /// <summary>
    /// 注册联锁规则
    /// </summary>
    void RegisterRule(InterlockRule rule);

    /// <summary>
    /// 批量注册联锁规则
    /// </summary>
    void RegisterRules(IEnumerable<InterlockRule> rules);

    /// <summary>
    /// 移除联锁规则
    /// </summary>
    bool RemoveRule(string ruleId);

    /// <summary>
    /// 清除所有规则
    /// </summary>
    void ClearAllRules();

    /// <summary>
    /// 获取所有规则
    /// </summary>
    IReadOnlyList<InterlockRule> GetAllRules();

    /// <summary>
    /// 获取指定类型的规则
    /// </summary>
    IReadOnlyList<InterlockRule> GetRulesByType(InterlockTypeEnum type);

    /// <summary>
    /// 启用/禁用指定规则
    /// </summary>
    void SetRuleEnabled(string ruleId, bool enabled);

    /// <summary>
    /// 评估联锁条件
    /// </summary>
    /// <param name="signalValues">当前信号值字典</param>
    /// <returns>评估后触发的联锁列表</returns>
    IReadOnlyList<InterlockState> EvaluateInterlocks(IReadOnlyDictionary<string, double> signalValues);

    /// <summary>
    /// 评估单个规则
    /// </summary>
    InterlockState? EvaluateRule(string ruleId, IReadOnlyDictionary<string, double> signalValues);

    /// <summary>
    /// 获取所有联锁状态
    /// </summary>
    IReadOnlyList<InterlockState> GetAllInterlockStates();

    /// <summary>
    /// 获取所有触发的联锁
    /// </summary>
    IReadOnlyList<InterlockState> GetTrippedInterlocks();

    /// <summary>
    /// 获取所有锁存的联锁
    /// </summary>
    IReadOnlyList<InterlockState> GetLatchedInterlocks();

    /// <summary>
    /// 尝试清除联锁
    /// </summary>
    /// <param name="ruleId">规则ID</param>
    /// <param name="operatorId">操作员ID</param>
    /// <param name="reason">清除原因</param>
    /// <param name="signalValues">当前信号值（用于验证条件是否已恢复）</param>
    /// <returns>是否成功清除</returns>
    Task<(bool Success, string Message)> TryClearInterlockAsync(
        string ruleId,
        string operatorId,
        string reason,
        IReadOnlyDictionary<string, double>? signalValues = null);

    /// <summary>
    /// 尝试批量清除联锁
    /// </summary>
    Task<IReadOnlyList<(string RuleId, bool Success, string Message)>> TryClearInterlocksAsync(
        IEnumerable<string> ruleIds,
        string operatorId,
        string reason,
        IReadOnlyDictionary<string, double>? signalValues = null);

    /// <summary>
    /// 检查是否有任何联锁触发
    /// </summary>
    bool HasAnyTrippedInterlocks { get; }

    /// <summary>
    /// 检查是否有任何锁存的联锁
    /// </summary>
    bool HasAnyLatchedInterlocks { get; }

    /// <summary>
    /// 获取联锁导致的禁用能力
    /// </summary>
    CapabilityFlags GetDisabledCapabilities();

    /// <summary>
    /// 检查指定操作是否被联锁阻止
    /// </summary>
    (bool Blocked, IReadOnlyList<string> BlockingRuleIds) IsOperationBlocked(CapabilityFlags requiredCapabilities);
}
