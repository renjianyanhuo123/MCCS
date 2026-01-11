using System.Collections.Concurrent;
using System.Reactive.Subjects;
using MCCS.Station.Abstractions.Enums;
using MCCS.Station.Abstractions.Events;
using MCCS.Station.Abstractions.Interfaces;
using MCCS.Station.Abstractions.Models;

namespace MCCS.Station.Core.Safety;

/// <summary>
/// 联锁引擎
/// 负责防止危险动作发生，强调"条件不满足就禁止"
/// 联锁通常需要明确的复位动作
/// </summary>
public sealed class InterlockEngine : IInterlockEngine, IDisposable
{
    private readonly object _lock = new();
    private readonly ConcurrentDictionary<string, InterlockRule> _rules = new();
    private readonly ConcurrentDictionary<string, InterlockState> _states = new();
    private readonly Subject<InterlockTriggeredEvent> _interlockTriggered = new();

    public IObservable<InterlockTriggeredEvent> InterlockTriggered => _interlockTriggered;

    public void RegisterRule(InterlockRule rule)
    {
        if (string.IsNullOrEmpty(rule.RuleId))
            throw new ArgumentException("RuleId cannot be empty", nameof(rule));

        _rules[rule.RuleId] = rule;

        // 初始化状态
        _states[rule.RuleId] = new InterlockState
        {
            Rule = rule,
            IsTripped = false,
            IsLatched = false,
            TripReason = string.Empty,
            ClearInstructions = GetClearInstructions(rule)
        };

        Console.WriteLine($"[InterlockEngine] 注册联锁规则: {rule.RuleId} ({rule.Name}) - 类型: {rule.Type}");
    }

    public void RegisterRules(IEnumerable<InterlockRule> rules)
    {
        foreach (var rule in rules)
        {
            RegisterRule(rule);
        }
    }

    public bool RemoveRule(string ruleId)
    {
        var removed = _rules.TryRemove(ruleId, out _);
        if (removed)
        {
            _states.TryRemove(ruleId, out _);
            Console.WriteLine($"[InterlockEngine] 移除联锁规则: {ruleId}");
        }
        return removed;
    }

    public void ClearAllRules()
    {
        _rules.Clear();
        _states.Clear();
        Console.WriteLine("[InterlockEngine] 清除所有联锁规则");
    }

    public IReadOnlyList<InterlockRule> GetAllRules()
    {
        return _rules.Values.ToList().AsReadOnly();
    }

    public IReadOnlyList<InterlockRule> GetRulesByType(InterlockTypeEnum type)
    {
        return _rules.Values
            .Where(r => r.Type == type)
            .ToList()
            .AsReadOnly();
    }

    public void SetRuleEnabled(string ruleId, bool enabled)
    {
        if (_rules.TryGetValue(ruleId, out var rule))
        {
            var newRule = new InterlockRule
            {
                RuleId = rule.RuleId,
                Name = rule.Name,
                Type = rule.Type,
                Description = rule.Description,
                SourceSignals = rule.SourceSignals,
                ConditionExpression = rule.ConditionExpression,
                IsLatched = rule.IsLatched,
                ResetPolicy = rule.ResetPolicy,
                DisabledCapabilities = rule.DisabledCapabilities,
                TriggerAction = rule.TriggerAction,
                Priority = rule.Priority,
                IsEnabled = enabled,
                Metadata = rule.Metadata
            };
            _rules[ruleId] = newRule;

            // 更新状态
            if (_states.TryGetValue(ruleId, out var state))
            {
                _states[ruleId] = new InterlockState
                {
                    Rule = newRule,
                    IsTripped = enabled ? state.IsTripped : false,
                    IsLatched = enabled ? state.IsLatched : false,
                    TrippedAt = enabled ? state.TrippedAt : null,
                    TripReason = enabled ? state.TripReason : string.Empty,
                    TriggerValues = enabled ? state.TriggerValues : null,
                    ClearInstructions = GetClearInstructions(newRule)
                };
            }

            Console.WriteLine($"[InterlockEngine] 联锁规则 {ruleId} {(enabled ? "已启用" : "已禁用")}");
        }
    }

    public IReadOnlyList<InterlockState> EvaluateInterlocks(IReadOnlyDictionary<string, double> signalValues)
    {
        var triggered = new List<InterlockState>();

        foreach (var rule in _rules.Values.Where(r => r.IsEnabled).OrderByDescending(r => r.Priority))
        {
            var state = EvaluateRuleInternal(rule, signalValues);
            if (state != null)
            {
                _states[rule.RuleId] = state;
                if (state.IsTripped)
                {
                    triggered.Add(state);
                }
            }
        }

        return triggered.AsReadOnly();
    }

    public InterlockState? EvaluateRule(string ruleId, IReadOnlyDictionary<string, double> signalValues)
    {
        if (!_rules.TryGetValue(ruleId, out var rule) || !rule.IsEnabled)
        {
            return null;
        }

        var state = EvaluateRuleInternal(rule, signalValues);
        if (state != null)
        {
            _states[rule.RuleId] = state;
        }

        return state;
    }

    private InterlockState? EvaluateRuleInternal(InterlockRule rule, IReadOnlyDictionary<string, double> signalValues)
    {
        var currentState = _states.GetValueOrDefault(rule.RuleId);
        var wasTripped = currentState?.IsTripped ?? false;
        var wasLatched = currentState?.IsLatched ?? false;

        // 评估条件
        var isConditionTripped = EvaluateCondition(rule, signalValues);
        var isTripped = isConditionTripped;

        // 如果是锁存模式且之前已锁存，保持触发状态
        if (rule.IsLatched && wasLatched)
        {
            isTripped = true;
        }

        var now = DateTime.UtcNow;

        // 构建新状态
        var newState = new InterlockState
        {
            Rule = rule,
            IsTripped = isTripped,
            IsLatched = rule.IsLatched && isTripped,
            TrippedAt = isTripped ? (currentState?.TrippedAt ?? now) : null,
            TripReason = isTripped ? GenerateTripReason(rule, signalValues) : string.Empty,
            TriggerValues = isTripped ? signalValues.ToDictionary(kv => kv.Key, kv => kv.Value) : null,
            ClearInstructions = GetClearInstructions(rule)
        };

        // 发布事件
        if (isTripped && !wasTripped)
        {
            // 新触发
            var evt = new InterlockTriggeredEvent
            {
                RuleId = rule.RuleId,
                InterlockType = rule.Type,
                IsTripped = true,
                IsLatched = newState.IsLatched,
                Reason = newState.TripReason,
                TriggerValues = newState.TriggerValues,
                ActionTaken = rule.TriggerAction,
                DisabledCapabilities = rule.DisabledCapabilities,
                ResetPolicy = rule.ResetPolicy,
                ClearInstructions = newState.ClearInstructions,
                Source = "InterlockEngine"
            };

            Console.WriteLine($"[InterlockEngine] 联锁触发: {rule.Name} ({rule.RuleId}) - {newState.TripReason}");
            _interlockTriggered.OnNext(evt);
        }
        else if (!isTripped && wasTripped)
        {
            // 已清除
            var evt = new InterlockTriggeredEvent
            {
                RuleId = rule.RuleId,
                InterlockType = rule.Type,
                IsTripped = false,
                IsLatched = false,
                Reason = "联锁已清除",
                ActionTaken = InterlockAction.Hold,
                DisabledCapabilities = CapabilityFlags.None,
                ResetPolicy = rule.ResetPolicy,
                ClearInstructions = string.Empty,
                Source = "InterlockEngine"
            };

            Console.WriteLine($"[InterlockEngine] 联锁清除: {rule.Name} ({rule.RuleId})");
            _interlockTriggered.OnNext(evt);
        }

        return newState;
    }

    private bool EvaluateCondition(InterlockRule rule, IReadOnlyDictionary<string, double> signalValues)
    {
        // 简化的条件评估
        // 实际实现中可以使用表达式引擎（如 NCalc）来评估复杂条件
        if (string.IsNullOrEmpty(rule.ConditionExpression))
        {
            return false;
        }

        try
        {
            // 支持简单的条件格式：
            // "signal_name < threshold" 或 "signal_name > threshold" 或 "signal_name == value"
            var expr = rule.ConditionExpression.Trim();

            // 处理常见的联锁类型
            switch (rule.Type)
            {
                case InterlockTypeEnum.EStop:
                    // 急停信号：如果有 estop 信号且值为 1（或非零），则触发
                    if (signalValues.TryGetValue("estop", out var estopValue))
                    {
                        return estopValue != 0;
                    }
                    break;

                case InterlockTypeEnum.DoorOpen:
                    // 门禁信号：如果 door_closed 信号为 0，则触发
                    if (signalValues.TryGetValue("door_closed", out var doorValue))
                    {
                        return doorValue == 0;
                    }
                    break;

                case InterlockTypeEnum.OilPressure:
                    // 油压检查
                    if (signalValues.TryGetValue("oil_pressure", out var pressureValue))
                    {
                        // 假设表达式为 "oil_pressure < 100"
                        if (expr.Contains("<"))
                        {
                            var parts = expr.Split('<');
                            if (parts.Length == 2 && double.TryParse(parts[1].Trim(), out var threshold))
                            {
                                return pressureValue < threshold;
                            }
                        }
                    }
                    break;

                case InterlockTypeEnum.OilTemperature:
                    // 油温检查
                    if (signalValues.TryGetValue("oil_temperature", out var tempValue))
                    {
                        if (expr.Contains(">"))
                        {
                            var parts = expr.Split('>');
                            if (parts.Length == 2 && double.TryParse(parts[1].Trim(), out var threshold))
                            {
                                return tempValue > threshold;
                            }
                        }
                    }
                    break;

                case InterlockTypeEnum.TravelLimit:
                    // 行程限位
                    foreach (var signal in rule.SourceSignals)
                    {
                        if (signalValues.TryGetValue(signal, out var value) && value != 0)
                        {
                            return true;
                        }
                    }
                    break;

                case InterlockTypeEnum.GripClosed:
                    // 夹具闭合
                    if (signalValues.TryGetValue("grip_closed", out var gripValue))
                    {
                        return gripValue == 0; // 夹具未闭合时触发
                    }
                    break;
            }

            // 通用表达式解析
            return EvaluateGenericExpression(expr, signalValues);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[InterlockEngine] 条件评估错误 ({rule.RuleId}): {ex.Message}");
            return false;
        }
    }

    private static bool EvaluateGenericExpression(string expr, IReadOnlyDictionary<string, double> signalValues)
    {
        // 简单的表达式解析器
        // 格式：signal_name operator value
        var operators = new[] { ">=", "<=", "!=", "==", ">", "<" };

        foreach (var op in operators)
        {
            var index = expr.IndexOf(op, StringComparison.Ordinal);
            if (index > 0)
            {
                var signalName = expr.Substring(0, index).Trim();
                var valueStr = expr.Substring(index + op.Length).Trim();

                if (signalValues.TryGetValue(signalName, out var signalValue) &&
                    double.TryParse(valueStr, out var compareValue))
                {
                    return op switch
                    {
                        ">=" => signalValue >= compareValue,
                        "<=" => signalValue <= compareValue,
                        "!=" => Math.Abs(signalValue - compareValue) > 0.0001,
                        "==" => Math.Abs(signalValue - compareValue) < 0.0001,
                        ">" => signalValue > compareValue,
                        "<" => signalValue < compareValue,
                        _ => false
                    };
                }
            }
        }

        return false;
    }

    private static string GenerateTripReason(InterlockRule rule, IReadOnlyDictionary<string, double> signalValues)
    {
        var reason = $"{rule.Name}: {rule.Description}";

        if (rule.SourceSignals.Count > 0)
        {
            var values = rule.SourceSignals
                .Where(s => signalValues.ContainsKey(s))
                .Select(s => $"{s}={signalValues[s]:F2}");
            reason += $" [信号值: {string.Join(", ", values)}]";
        }

        return reason;
    }

    private static string GetClearInstructions(InterlockRule rule)
    {
        return rule.ResetPolicy switch
        {
            InterlockResetPolicy.Auto => "条件恢复后将自动清除",
            InterlockResetPolicy.Manual => "请确认条件已恢复，然后点击清除联锁",
            InterlockResetPolicy.ManualWithConfirm => "请确认条件已恢复并输入原因，然后点击清除联锁",
            InterlockResetPolicy.Hardware => "请复位硬件（如按下急停复位按钮），然后点击清除联锁",
            InterlockResetPolicy.Maintenance => "请联系维护人员进行检查和复位",
            _ => "请确认条件已恢复后清除联锁"
        };
    }

    public IReadOnlyList<InterlockState> GetAllInterlockStates()
    {
        return _states.Values.ToList().AsReadOnly();
    }

    public IReadOnlyList<InterlockState> GetTrippedInterlocks()
    {
        return _states.Values
            .Where(s => s.IsTripped)
            .OrderByDescending(s => s.Rule.Priority)
            .ToList()
            .AsReadOnly();
    }

    public IReadOnlyList<InterlockState> GetLatchedInterlocks()
    {
        return _states.Values
            .Where(s => s.IsLatched)
            .OrderByDescending(s => s.Rule.Priority)
            .ToList()
            .AsReadOnly();
    }

    public async Task<(bool Success, string Message)> TryClearInterlockAsync(
        string ruleId,
        string operatorId,
        string reason,
        IReadOnlyDictionary<string, double>? signalValues = null)
    {
        if (!_states.TryGetValue(ruleId, out var state))
        {
            return (false, $"联锁规则 {ruleId} 不存在");
        }

        if (!state.IsTripped)
        {
            return (false, $"联锁 {ruleId} 未触发，无需清除");
        }

        var rule = state.Rule;

        // 检查复位策略
        if (rule.ResetPolicy == InterlockResetPolicy.Hardware)
        {
            return (false, "此联锁需要硬件复位，无法通过软件清除");
        }

        if (rule.ResetPolicy == InterlockResetPolicy.Maintenance)
        {
            return (false, "此联锁需要维护人员处理，无法直接清除");
        }

        // 如果提供了信号值，检查条件是否已恢复
        if (signalValues != null && rule.ResetPolicy != InterlockResetPolicy.Auto)
        {
            var stillTripped = EvaluateCondition(rule, signalValues);
            if (stillTripped)
            {
                return (false, "联锁条件尚未恢复，请先解决触发条件");
            }
        }

        // 清除联锁
        _states[ruleId] = new InterlockState
        {
            Rule = rule,
            IsTripped = false,
            IsLatched = false,
            TrippedAt = null,
            TripReason = string.Empty,
            TriggerValues = null,
            ClearInstructions = GetClearInstructions(rule)
        };

        // 发布事件
        var evt = new InterlockTriggeredEvent
        {
            RuleId = rule.RuleId,
            InterlockType = rule.Type,
            IsTripped = false,
            IsLatched = false,
            Reason = $"由 {operatorId} 清除: {reason}",
            ActionTaken = InterlockAction.Hold,
            DisabledCapabilities = CapabilityFlags.None,
            ResetPolicy = rule.ResetPolicy,
            ClearInstructions = string.Empty,
            Source = "InterlockEngine"
        };

        Console.WriteLine($"[InterlockEngine] 联锁手动清除: {rule.Name} ({rule.RuleId}), 操作员={operatorId}, 原因={reason}");
        _interlockTriggered.OnNext(evt);

        await Task.CompletedTask;
        return (true, $"联锁 {rule.Name} 已成功清除");
    }

    public async Task<IReadOnlyList<(string RuleId, bool Success, string Message)>> TryClearInterlocksAsync(
        IEnumerable<string> ruleIds,
        string operatorId,
        string reason,
        IReadOnlyDictionary<string, double>? signalValues = null)
    {
        var results = new List<(string RuleId, bool Success, string Message)>();

        foreach (var ruleId in ruleIds)
        {
            var (success, message) = await TryClearInterlockAsync(ruleId, operatorId, reason, signalValues);
            results.Add((ruleId, success, message));
        }

        return results.AsReadOnly();
    }

    public bool HasAnyTrippedInterlocks => _states.Values.Any(s => s.IsTripped);

    public bool HasAnyLatchedInterlocks => _states.Values.Any(s => s.IsLatched);

    public CapabilityFlags GetDisabledCapabilities()
    {
        var disabled = CapabilityFlags.None;

        foreach (var state in _states.Values.Where(s => s.IsTripped))
        {
            disabled |= state.Rule.DisabledCapabilities;
        }

        return disabled;
    }

    public (bool Blocked, IReadOnlyList<string> BlockingRuleIds) IsOperationBlocked(CapabilityFlags requiredCapabilities)
    {
        var blockingRules = new List<string>();

        foreach (var state in _states.Values.Where(s => s.IsTripped))
        {
            if ((state.Rule.DisabledCapabilities & requiredCapabilities) != CapabilityFlags.None)
            {
                blockingRules.Add(state.Rule.RuleId);
            }
        }

        return (blockingRules.Count > 0, blockingRules.AsReadOnly());
    }

    public void Dispose()
    {
        _interlockTriggered.OnCompleted();
        _interlockTriggered.Dispose();
    }
}
