using System.Reactive.Subjects;
using MCCS.Station.Abstractions.Enums;
using MCCS.Station.Abstractions.Interfaces;

namespace MCCS.Station.Core.Safety;

/// <summary>
/// 流程状态机
/// 只管理流程状态（Idle→Armed→Running...），不涉及安全状态
/// 安全系统可以强制中断/降级流程状态
/// </summary>
public sealed class ProcessStateMachine : IProcessStateMachine, IDisposable
{
    private ProcessStatus _currentState = ProcessStatus.Idle;
    private readonly object _lock = new();
    private readonly Subject<ProcessStateChangedEvent> _stateChanged = new();
    private readonly IStatusAggregator? _statusAggregator;

    private volatile bool _isLockedBySafety;
    private string _safetyLockReason = string.Empty;

    /// <summary>
    /// 状态转换规则定义
    /// </summary>
    private static readonly Dictionary<ProcessStatus, HashSet<ProcessStatus>> ValidTransitions = new()
    {
        [ProcessStatus.Idle] = new() { ProcessStatus.Armed, ProcessStatus.Preparing, ProcessStatus.Manual },
        [ProcessStatus.Armed] = new() { ProcessStatus.Idle, ProcessStatus.Preparing, ProcessStatus.Running },
        [ProcessStatus.Preparing] = new() { ProcessStatus.Armed, ProcessStatus.Running, ProcessStatus.Idle },
        [ProcessStatus.Running] = new() { ProcessStatus.Paused, ProcessStatus.Stopping, ProcessStatus.Completed },
        [ProcessStatus.Paused] = new() { ProcessStatus.Running, ProcessStatus.Stopping, ProcessStatus.Idle },
        [ProcessStatus.Stopping] = new() { ProcessStatus.Idle, ProcessStatus.Completed, ProcessStatus.Unloading },
        [ProcessStatus.Completed] = new() { ProcessStatus.Idle, ProcessStatus.Unloading, ProcessStatus.Armed },
        [ProcessStatus.Unloading] = new() { ProcessStatus.Idle, ProcessStatus.Completed },
        [ProcessStatus.Manual] = new() { ProcessStatus.Idle }
    };

    public ProcessStateMachine(IStatusAggregator? statusAggregator = null)
    {
        _statusAggregator = statusAggregator;
    }

    public ProcessStatus CurrentState
    {
        get
        {
            lock (_lock)
            {
                return _currentState;
            }
        }
    }

    public IObservable<ProcessStateChangedEvent> StateChanged => _stateChanged;

    public bool IsOperational => CurrentState is ProcessStatus.Idle or ProcessStatus.Armed or
        ProcessStatus.Running or ProcessStatus.Paused or ProcessStatus.Manual;

    public bool IsRunning => CurrentState == ProcessStatus.Running;

    public bool CanStart => CurrentState is ProcessStatus.Idle or ProcessStatus.Armed or ProcessStatus.Completed
        && !_isLockedBySafety;

    public bool IsLockedBySafety
    {
        get
        {
            lock (_lock)
            {
                return _isLockedBySafety;
            }
        }
    }

    public bool TryTransition(ProcessStatus newState, string reason = "")
    {
        lock (_lock)
        {
            // 检查是否被安全系统锁定
            if (_isLockedBySafety && !IsAllowedWhenLocked(newState))
            {
                Console.WriteLine($"[ProcessStateMachine] 状态机被安全锁定，禁止转换: {_currentState} -> {newState}");
                return false;
            }

            if (!CanTransitionTo(newState))
            {
                Console.WriteLine($"[ProcessStateMachine] 无效转换: {_currentState} -> {newState}");
                return false;
            }

            var previousState = _currentState;
            _currentState = newState;

            // 更新状态聚合器
            _statusAggregator?.UpdateProcess(newState, reason);

            var evt = new ProcessStateChangedEvent
            {
                PreviousState = previousState,
                CurrentState = newState,
                Reason = reason,
                ForcedBySafety = false,
                TriggeredBy = "User",
                Source = "ProcessStateMachine"
            };

            Console.WriteLine($"[ProcessStateMachine] {previousState} -> {newState} ({reason})");
            _stateChanged.OnNext(evt);
            return true;
        }
    }

    public void ForceTransition(ProcessStatus newState, string reason, string triggeredBy)
    {
        lock (_lock)
        {
            var previousState = _currentState;
            _currentState = newState;

            // 更新状态聚合器
            _statusAggregator?.UpdateProcess(newState, reason);

            var evt = new ProcessStateChangedEvent
            {
                PreviousState = previousState,
                CurrentState = newState,
                Reason = reason,
                ForcedBySafety = true,
                TriggeredBy = triggeredBy,
                Source = "ProcessStateMachine"
            };

            Console.WriteLine($"[ProcessStateMachine] 强制转换: {previousState} -> {newState} (由 {triggeredBy}: {reason})");
            _stateChanged.OnNext(evt);
        }
    }

    public bool CanTransitionTo(ProcessStatus targetState)
    {
        lock (_lock)
        {
            return ValidTransitions.TryGetValue(_currentState, out var validTargets)
                   && validTargets.Contains(targetState);
        }
    }

    public IReadOnlyList<ProcessStatus> GetAllowedTransitions()
    {
        lock (_lock)
        {
            if (ValidTransitions.TryGetValue(_currentState, out var validTargets))
            {
                if (_isLockedBySafety)
                {
                    // 被锁定时只允许安全相关的转换
                    return validTargets
                        .Where(IsAllowedWhenLocked)
                        .ToList()
                        .AsReadOnly();
                }
                return validTargets.ToList().AsReadOnly();
            }
            return Array.Empty<ProcessStatus>();
        }
    }

    public void SetSafetyLock(bool locked, string reason)
    {
        lock (_lock)
        {
            if (_isLockedBySafety == locked)
            {
                return;
            }

            _isLockedBySafety = locked;
            _safetyLockReason = reason;

            if (locked)
            {
                Console.WriteLine($"[ProcessStateMachine] 安全锁定: {reason}");

                // 如果正在运行，强制转换到暂停或停止
                if (_currentState == ProcessStatus.Running)
                {
                    ForceTransition(ProcessStatus.Paused, $"安全锁定: {reason}", "SafetySystem");
                }
            }
            else
            {
                Console.WriteLine($"[ProcessStateMachine] 安全锁定解除: {reason}");
            }
        }
    }

    private static bool IsAllowedWhenLocked(ProcessStatus status)
    {
        // 被安全锁定时，只允许转换到安全状态
        return status is ProcessStatus.Idle or ProcessStatus.Paused or ProcessStatus.Stopping;
    }

    public void Dispose()
    {
        _stateChanged.OnCompleted();
        _stateChanged.Dispose();
    }
}
