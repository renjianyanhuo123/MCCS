using MCCS.Station.Abstractions.Enums;
using MCCS.Station.Abstractions.Events;

namespace MCCS.Station.Abstractions.Interfaces;

/// <summary>
/// 流程状态机接口
/// 只管理流程状态（Idle→Armed→Running...），不涉及安全状态
/// 安全系统可以强制中断/降级流程状态
/// </summary>
public interface IProcessStateMachine
{
    /// <summary>
    /// 当前流程状态
    /// </summary>
    ProcessStatus CurrentState { get; }

    /// <summary>
    /// 状态变更事件流
    /// </summary>
    IObservable<ProcessStateChangedEvent> StateChanged { get; }

    /// <summary>
    /// 尝试转换到新状态
    /// </summary>
    /// <param name="newState">目标状态</param>
    /// <param name="reason">转换原因</param>
    /// <returns>是否成功转换</returns>
    bool TryTransition(ProcessStatus newState, string reason = "");

    /// <summary>
    /// 强制转换到新状态（由安全系统调用）
    /// </summary>
    /// <param name="newState">目标状态</param>
    /// <param name="reason">转换原因</param>
    /// <param name="triggeredBy">触发源（如 "SafetySupervisor"）</param>
    void ForceTransition(ProcessStatus newState, string reason, string triggeredBy);

    /// <summary>
    /// 检查是否可以转换到目标状态
    /// </summary>
    bool CanTransitionTo(ProcessStatus targetState);

    /// <summary>
    /// 获取当前状态允许的目标状态
    /// </summary>
    IReadOnlyList<ProcessStatus> GetAllowedTransitions();

    /// <summary>
    /// 是否处于可运行状态
    /// </summary>
    bool IsOperational { get; }

    /// <summary>
    /// 是否正在执行试验
    /// </summary>
    bool IsRunning { get; }

    /// <summary>
    /// 是否可以启动试验
    /// </summary>
    bool CanStart { get; }

    /// <summary>
    /// 是否被安全系统锁定
    /// </summary>
    bool IsLockedBySafety { get; }

    /// <summary>
    /// 设置安全锁定状态
    /// </summary>
    void SetSafetyLock(bool locked, string reason);
}

/// <summary>
/// 流程状态变更事件
/// </summary>
public record ProcessStateChangedEvent : StationEvent
{
    /// <summary>
    /// 之前的状态
    /// </summary>
    public ProcessStatus PreviousState { get; init; }

    /// <summary>
    /// 当前状态
    /// </summary>
    public ProcessStatus CurrentState { get; init; }

    /// <summary>
    /// 变更原因
    /// </summary>
    public string Reason { get; init; } = string.Empty;

    /// <summary>
    /// 是否由安全系统强制触发
    /// </summary>
    public bool ForcedBySafety { get; init; }

    /// <summary>
    /// 触发源
    /// </summary>
    public string TriggeredBy { get; init; } = string.Empty;
}
