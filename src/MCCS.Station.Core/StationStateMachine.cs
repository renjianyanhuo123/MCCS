using System.Reactive.Subjects;

using MCCS.Station.Abstractions.Enums;
using MCCS.Station.Abstractions.Events;

namespace MCCS.Station.Core;

/// <summary>
/// 站点状态机 - 管理站点生命周期状态
/// </summary>
public class StationStateMachine
{
    private StationStateEnum _currentState = StationStateEnum.Offline;
    private readonly object _lock = new();
    private readonly Subject<StateChangedEvent> _stateChanged = new();

    public StationStateEnum CurrentState
    {
        get { lock (_lock) return _currentState; }
    }

    public IObservable<StateChangedEvent> StateChanged => _stateChanged;

    /// <summary>
    /// 状态转换规则定义
    /// </summary>
    // ReSharper disable once InconsistentNaming
    private static readonly Dictionary<StationStateEnum, HashSet<StationStateEnum>> ValidTransitions = new()
    {
        [StationStateEnum.Offline] = [StationStateEnum.Connecting],
        [StationStateEnum.Connecting] = [StationStateEnum.Online, StationStateEnum.Offline, StationStateEnum.Faulted],
        [StationStateEnum.Online] = [StationStateEnum.Ready, StationStateEnum.Offline, StationStateEnum.Faulted],
        [StationStateEnum.Ready] = [StationStateEnum.Running, StationStateEnum.Offline, StationStateEnum.Faulted, StationStateEnum.EStop],
        [StationStateEnum.Running] = [StationStateEnum.Ready, StationStateEnum.Paused, StationStateEnum.Faulted, StationStateEnum.EStop],
        [StationStateEnum.Paused] = [StationStateEnum.Running, StationStateEnum.Ready, StationStateEnum.Faulted, StationStateEnum.EStop],
        [StationStateEnum.Faulted] = [StationStateEnum.Recovering, StationStateEnum.Offline],
        [StationStateEnum.EStop] = [StationStateEnum.Recovering, StationStateEnum.Offline],
        [StationStateEnum.Recovering] = [StationStateEnum.Ready, StationStateEnum.Faulted, StationStateEnum.Offline]
    };

    /// <summary>
    /// 尝试切换状态
    /// </summary>
    public bool TryTransition(StationStateEnum newState, string reason = "")
    {
        lock (_lock)
        {
            if (!CanTransitionTo(newState))
            {
                Console.WriteLine($"[StateMachine] Invalid transition: {_currentState} -> {newState}");
                return false;
            }

            var previousState = _currentState;
            _currentState = newState;

            var evt = new StateChangedEvent
            {
                PreviousState = previousState,
                CurrentState = newState,
                Reason = reason,
                Source = "StateMachine"
            };

            Console.WriteLine($"[StateMachine] {previousState} -> {newState} ({reason})");
            _stateChanged.OnNext(evt);
            return true;
        }
    }

    /// <summary>
    /// 检查是否可以转换到目标状态
    /// </summary>
    public bool CanTransitionTo(StationStateEnum targetState)
    {
        lock (_lock)
        {
            return ValidTransitions.TryGetValue(_currentState, out var validTargets)
                   && validTargets.Contains(targetState);
        }
    }

    /// <summary>
    /// 检查当前是否处于可运行状态
    /// </summary>
    public bool IsOperational => CurrentState is StationStateEnum.Ready or StationStateEnum.Running or StationStateEnum.Paused;
}
