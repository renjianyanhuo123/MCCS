namespace MCCS.Infrastructure.TestModels;

public sealed class StateMachine<TState>(TState initialState)
    where TState : Enum
{
    private readonly Dictionary<(TState From, TState To), Action?> _transitions = new();
    
    public TState CurrentState { get; private set; } = initialState;

    public event Action<TState, TState>? StateChanged;

    /// <summary>
    /// 配置允许的状态转换
    /// </summary>
    /// <param name="from">变更前状态</param>
    /// <param name="to">变更后状态</param>
    /// <param name="onTransition"></param>
    public void AddTransition(TState from, TState to, Action? onTransition = null) => _transitions[(from, to)] = onTransition;

    /// <summary>
    /// 尝试转换状态
    /// </summary>
    /// <param name="newState"></param>
    /// <returns></returns>
    public bool TryTransition(TState newState)
    {
        if (EqualityComparer<TState>.Default.Equals(CurrentState, newState))
            return true; // 已经是目标状态

        var key = (CurrentState, newState);
        if (!_transitions.ContainsKey(key))
            return false; // 不允许的转换

        var oldState = CurrentState;
        _transitions[key]?.Invoke(); // 执行转换回调
        CurrentState = newState;
        StateChanged?.Invoke(oldState, newState);
        return true;
    }

    /// <summary>
    /// 强制转换（不检查规则）
    /// </summary>
    /// <param name="newState"></param>
    public void ForceTransition(TState newState)
    {
        if (EqualityComparer<TState>.Default.Equals(CurrentState, newState))
            return;

        var oldState = CurrentState;
        CurrentState = newState;
        StateChanged?.Invoke(oldState, newState);
    }
}
