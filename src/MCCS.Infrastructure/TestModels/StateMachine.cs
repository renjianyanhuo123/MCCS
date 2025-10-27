namespace MCCS.Infrastructure.TestModels;

public sealed class StateMachine<TState>(TState initialState)
    where TState : Enum
{
    private TState _currentState = initialState;
    private readonly Dictionary<(TState From, TState To), Action?> _transitions = new();
    
    public TState CurrentState => _currentState;
    public event Action<TState, TState>? StateChanged;

    /// <summary>
    /// 配置允许的状态转换
    /// </summary>
    /// <param name="from">变更前状态</param>
    /// <param name="to">变更后状态</param>
    /// <param name="onTransition"></param>
    public void AddTransition(TState from, TState to, Action? onTransition = null)
    {
        _transitions[(from, to)] = onTransition;
    }

    /// <summary>
    /// 尝试转换状态
    /// </summary>
    /// <param name="newState"></param>
    /// <returns></returns>
    public bool TryTransition(TState newState)
    {
        if (EqualityComparer<TState>.Default.Equals(_currentState, newState))
            return true; // 已经是目标状态

        var key = (_currentState, newState);
        if (!_transitions.ContainsKey(key))
            return false; // 不允许的转换

        var oldState = _currentState;
        _transitions[key]?.Invoke(); // 执行转换回调
        _currentState = newState;
        StateChanged?.Invoke(oldState, newState);
        return true;
    }

    /// <summary>
    /// 强制转换（不检查规则）
    /// </summary>
    /// <param name="newState"></param>
    public void ForceTransition(TState newState)
    {
        if (EqualityComparer<TState>.Default.Equals(_currentState, newState))
            return;

        var oldState = _currentState;
        _currentState = newState;
        StateChanged?.Invoke(oldState, newState);
    }
}
