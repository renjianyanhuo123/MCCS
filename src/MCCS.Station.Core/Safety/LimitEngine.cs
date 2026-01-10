using System.Collections.Concurrent;
using System.Reactive.Subjects;
using MCCS.Station.Abstractions.Enums;
using MCCS.Station.Abstractions.Events;
using MCCS.Station.Abstractions.Interfaces;
using MCCS.Station.Abstractions.Models;

namespace MCCS.Station.Core.Safety;

/// <summary>
/// 软限位引擎
/// 负责过程保护，保护试样/过程，系统仍可控
/// 典型策略：超限触发时把相关轴切到"位移保持/力保持/速度限制"
/// </summary>
public sealed class LimitEngine : ILimitEngine, IDisposable
{
    private readonly object _lock = new();
    private readonly ConcurrentDictionary<string, SoftLimitConfig> _configs = new();
    private readonly ConcurrentDictionary<string, SoftLimitState> _states = new();
    private readonly Subject<SoftLimitTriggeredEvent> _limitTriggered = new();
    private readonly Subject<LimitTrippedEvent> _limitWarning = new();

    public IObservable<SoftLimitTriggeredEvent> LimitTriggered => _limitTriggered;
    public IObservable<LimitTrippedEvent> LimitWarning => _limitWarning;

    public void RegisterLimit(SoftLimitConfig config)
    {
        if (string.IsNullOrEmpty(config.LimitId))
            throw new ArgumentException("LimitId cannot be empty", nameof(config));

        _configs[config.LimitId] = config;

        // 初始化状态
        _states[config.LimitId] = new SoftLimitState
        {
            Config = config,
            CurrentValue = 0,
            IsWarning = false,
            IsTripped = false,
            IsUpperTripped = false,
            IsLowerTripped = false
        };

        Console.WriteLine($"[LimitEngine] 注册限位: {config.LimitId} ({config.Name}) - 通道: {config.ChannelId}");
    }

    public void RegisterLimits(IEnumerable<SoftLimitConfig> configs)
    {
        foreach (var config in configs)
        {
            RegisterLimit(config);
        }
    }

    public bool RemoveLimit(string limitId)
    {
        var removed = _configs.TryRemove(limitId, out _);
        if (removed)
        {
            _states.TryRemove(limitId, out _);
            Console.WriteLine($"[LimitEngine] 移除限位: {limitId}");
        }
        return removed;
    }

    public void ClearAllLimits()
    {
        _configs.Clear();
        _states.Clear();
        Console.WriteLine("[LimitEngine] 清除所有限位");
    }

    public IReadOnlyList<SoftLimitConfig> GetAllLimits()
    {
        return _configs.Values.ToList().AsReadOnly();
    }

    public IReadOnlyList<SoftLimitConfig> GetLimitsForChannel(string channelId)
    {
        return _configs.Values
            .Where(c => c.ChannelId == channelId)
            .ToList()
            .AsReadOnly();
    }

    public void SetLimitEnabled(string limitId, bool enabled)
    {
        if (_configs.TryGetValue(limitId, out var config))
        {
            // 创建新的配置副本
            var newConfig = new SoftLimitConfig
            {
                LimitId = config.LimitId,
                Name = config.Name,
                ChannelId = config.ChannelId,
                SignalName = config.SignalName,
                LimitType = config.LimitType,
                UpperLimit = config.UpperLimit,
                LowerLimit = config.LowerLimit,
                WarningThreshold = config.WarningThreshold,
                TriggerAction = config.TriggerAction,
                AutoRelease = config.AutoRelease,
                ReleaseThreshold = config.ReleaseThreshold,
                IsEnabled = enabled,
                Unit = config.Unit,
                Priority = config.Priority
            };
            _configs[limitId] = newConfig;

            // 更新状态
            if (_states.TryGetValue(limitId, out var state))
            {
                _states[limitId] = new SoftLimitState
                {
                    Config = newConfig,
                    CurrentValue = state.CurrentValue,
                    IsWarning = enabled ? state.IsWarning : false,
                    IsTripped = enabled ? state.IsTripped : false,
                    IsUpperTripped = enabled ? state.IsUpperTripped : false,
                    IsLowerTripped = enabled ? state.IsLowerTripped : false,
                    TrippedAt = enabled ? state.TrippedAt : null,
                    TripValue = enabled ? state.TripValue : null,
                    MarginToUpper = state.MarginToUpper,
                    MarginToLower = state.MarginToLower,
                    ActiveAction = enabled ? state.ActiveAction : null
                };
            }

            Console.WriteLine($"[LimitEngine] 限位 {limitId} {(enabled ? "已启用" : "已禁用")}");
        }
    }

    public void UpdateLimitThreshold(string limitId, double? upperLimit = null, double? lowerLimit = null)
    {
        if (_configs.TryGetValue(limitId, out var config))
        {
            var newConfig = new SoftLimitConfig
            {
                LimitId = config.LimitId,
                Name = config.Name,
                ChannelId = config.ChannelId,
                SignalName = config.SignalName,
                LimitType = config.LimitType,
                UpperLimit = upperLimit ?? config.UpperLimit,
                LowerLimit = lowerLimit ?? config.LowerLimit,
                WarningThreshold = config.WarningThreshold,
                TriggerAction = config.TriggerAction,
                AutoRelease = config.AutoRelease,
                ReleaseThreshold = config.ReleaseThreshold,
                IsEnabled = config.IsEnabled,
                Unit = config.Unit,
                Priority = config.Priority
            };
            _configs[limitId] = newConfig;

            // 更新状态中的配置引用
            if (_states.TryGetValue(limitId, out var state))
            {
                _states[limitId] = state with { Config = newConfig };
            }

            Console.WriteLine($"[LimitEngine] 更新限位阈值 {limitId}: 上限={upperLimit}, 下限={lowerLimit}");
        }
    }

    public SoftLimitState? EvaluateSignal(string channelId, string signalName, double value)
    {
        // 查找匹配的限位配置
        var matchingLimits = _configs.Values
            .Where(c => c.IsEnabled && c.ChannelId == channelId && c.SignalName == signalName)
            .OrderByDescending(c => c.Priority);

        SoftLimitState? result = null;

        foreach (var config in matchingLimits)
        {
            var state = EvaluateLimitInternal(config, value);
            _states[config.LimitId] = state;

            // 返回优先级最高的触发状态
            if (state.IsTripped && result == null)
            {
                result = state;
            }
        }

        return result;
    }

    public IReadOnlyList<SoftLimitState> EvaluateSignals(IEnumerable<(string ChannelId, string SignalName, double Value)> signals)
    {
        var results = new List<SoftLimitState>();

        foreach (var (channelId, signalName, value) in signals)
        {
            var state = EvaluateSignal(channelId, signalName, value);
            if (state != null)
            {
                results.Add(state);
            }
        }

        return results.AsReadOnly();
    }

    private SoftLimitState EvaluateLimitInternal(SoftLimitConfig config, double value)
    {
        var currentState = _states.GetValueOrDefault(config.LimitId);
        var wasTripped = currentState?.IsTripped ?? false;
        var wasWarning = currentState?.IsWarning ?? false;

        bool isUpperTripped = false;
        bool isLowerTripped = false;
        bool isWarning = false;
        double? marginToUpper = null;
        double? marginToLower = null;

        // 计算上限
        if (config.UpperLimit.HasValue)
        {
            marginToUpper = config.UpperLimit.Value - value;

            // 检查是否触发上限
            if (value >= config.UpperLimit.Value)
            {
                isUpperTripped = true;
            }
            // 检查是否进入警告区域
            else if (value >= config.UpperLimit.Value * config.WarningThreshold)
            {
                isWarning = true;
            }

            // 如果之前触发了，检查是否可以自动解除
            if (wasTripped && config.AutoRelease)
            {
                if (value <= config.UpperLimit.Value * config.ReleaseThreshold)
                {
                    isUpperTripped = false;
                }
            }
        }

        // 计算下限
        if (config.LowerLimit.HasValue)
        {
            marginToLower = value - config.LowerLimit.Value;

            // 检查是否触发下限
            if (value <= config.LowerLimit.Value)
            {
                isLowerTripped = true;
            }
            // 检查是否进入警告区域
            else if (config.LowerLimit.Value != 0 && value <= config.LowerLimit.Value / config.WarningThreshold)
            {
                isWarning = true;
            }

            // 如果之前触发了，检查是否可以自动解除
            if (wasTripped && config.AutoRelease)
            {
                if (value >= config.LowerLimit.Value / config.ReleaseThreshold)
                {
                    isLowerTripped = false;
                }
            }
        }

        var isTripped = isUpperTripped || isLowerTripped;
        var now = DateTime.UtcNow;

        // 发布事件
        if (isTripped && !wasTripped)
        {
            // 新触发
            var overshoot = isUpperTripped
                ? value - config.UpperLimit!.Value
                : config.LowerLimit!.Value - value;

            var evt = new SoftLimitTriggeredEvent
            {
                LimitId = config.LimitId,
                ChannelId = config.ChannelId,
                SignalName = config.SignalName,
                LimitType = config.LimitType,
                IsTripped = true,
                IsUpperLimit = isUpperTripped,
                Threshold = isUpperTripped ? config.UpperLimit!.Value : config.LowerLimit!.Value,
                ActualValue = value,
                Overshoot = overshoot,
                ActionTaken = config.TriggerAction,
                Unit = config.Unit,
                Source = "LimitEngine"
            };

            Console.WriteLine($"[LimitEngine] 限位触发: {config.Name} ({config.LimitId}), 值={value}, 阈值={evt.Threshold}");
            _limitTriggered.OnNext(evt);
        }
        else if (!isTripped && wasTripped)
        {
            // 已解除
            var evt = new SoftLimitTriggeredEvent
            {
                LimitId = config.LimitId,
                ChannelId = config.ChannelId,
                SignalName = config.SignalName,
                LimitType = config.LimitType,
                IsTripped = false,
                IsUpperLimit = currentState?.IsUpperTripped ?? false,
                Threshold = (currentState?.IsUpperTripped ?? false) ? config.UpperLimit!.Value : config.LowerLimit!.Value,
                ActualValue = value,
                Overshoot = 0,
                ActionTaken = LimitAction.WarnOnly,
                Unit = config.Unit,
                Source = "LimitEngine"
            };

            Console.WriteLine($"[LimitEngine] 限位解除: {config.Name} ({config.LimitId}), 值={value}");
            _limitTriggered.OnNext(evt);
        }
        else if (isWarning && !wasWarning)
        {
            // 进入警告区域
            var warningEvt = new LimitTrippedEvent
            {
                ChannelId = config.ChannelId,
                SignalName = config.SignalName,
                Threshold = isUpperTripped || marginToUpper < marginToLower
                    ? config.UpperLimit!.Value
                    : config.LowerLimit!.Value,
                ActualValue = value,
                IsUpperLimit = marginToUpper < marginToLower,
                Source = "LimitEngine"
            };

            Console.WriteLine($"[LimitEngine] 限位警告: {config.Name} ({config.LimitId}), 值={value}");
            _limitWarning.OnNext(warningEvt);
        }

        return new SoftLimitState
        {
            Config = config,
            CurrentValue = value,
            IsWarning = isWarning,
            IsTripped = isTripped,
            IsUpperTripped = isUpperTripped,
            IsLowerTripped = isLowerTripped,
            TrippedAt = isTripped ? (currentState?.TrippedAt ?? now) : null,
            TripValue = isTripped ? (currentState?.TripValue ?? value) : null,
            MarginToUpper = marginToUpper,
            MarginToLower = marginToLower,
            ActiveAction = isTripped ? config.TriggerAction : null
        };
    }

    public IReadOnlyList<SoftLimitState> GetAllLimitStates()
    {
        return _states.Values.ToList().AsReadOnly();
    }

    public IReadOnlyList<SoftLimitState> GetTrippedLimits()
    {
        return _states.Values
            .Where(s => s.IsTripped)
            .OrderByDescending(s => s.Config.Priority)
            .ToList()
            .AsReadOnly();
    }

    public IReadOnlyList<SoftLimitState> GetWarningLimits()
    {
        return _states.Values
            .Where(s => s.IsWarning && !s.IsTripped)
            .OrderByDescending(s => s.Config.Priority)
            .ToList()
            .AsReadOnly();
    }

    public bool AcknowledgeLimit(string limitId, string operatorId, string reason)
    {
        if (!_states.TryGetValue(limitId, out var state) || !state.IsTripped)
        {
            return false;
        }

        // 对于需要确认的限位，操作员确认后解除
        if (!state.Config.AutoRelease)
        {
            _states[limitId] = state with
            {
                IsTripped = false,
                IsUpperTripped = false,
                IsLowerTripped = false,
                ActiveAction = null
            };

            var evt = new SoftLimitTriggeredEvent
            {
                LimitId = state.Config.LimitId,
                ChannelId = state.Config.ChannelId,
                SignalName = state.Config.SignalName,
                LimitType = state.Config.LimitType,
                IsTripped = false,
                IsUpperLimit = state.IsUpperTripped,
                Threshold = state.IsUpperTripped
                    ? state.Config.UpperLimit!.Value
                    : state.Config.LowerLimit!.Value,
                ActualValue = state.CurrentValue,
                Overshoot = 0,
                ActionTaken = LimitAction.WarnOnly,
                Unit = state.Config.Unit,
                Source = "LimitEngine"
            };

            Console.WriteLine($"[LimitEngine] 限位确认解除: {state.Config.Name}, 操作员={operatorId}, 原因={reason}");
            _limitTriggered.OnNext(evt);

            return true;
        }

        return false;
    }

    public bool HasAnyTrippedLimits => _states.Values.Any(s => s.IsTripped);

    public bool HasAnyWarnings => _states.Values.Any(s => s.IsWarning);

    public CapabilityFlags GetDisabledCapabilities()
    {
        var disabled = CapabilityFlags.None;

        foreach (var state in _states.Values.Where(s => s.IsTripped))
        {
            switch (state.ActiveAction)
            {
                case LimitAction.HoldPosition:
                case LimitAction.HoldLoad:
                case LimitAction.FreezeChannel:
                    disabled |= CapabilityFlags.CanMove | CapabilityFlags.CanStartTest;
                    break;

                case LimitAction.LimitSpeed:
                case LimitAction.BlockDirection:
                    disabled |= CapabilityFlags.CanStartTest;
                    break;

                case LimitAction.SoftStop:
                    disabled |= CapabilityFlags.CanMove | CapabilityFlags.CanStartTest | CapabilityFlags.CanResume;
                    break;

                case LimitAction.TriggerInterlock:
                    disabled |= CapabilityFlags.CanMove | CapabilityFlags.CanControl |
                               CapabilityFlags.CanStartTest | CapabilityFlags.CanResume;
                    break;

                case LimitAction.TriggerEStop:
                    // 由 EStopMonitor 处理
                    break;
            }
        }

        return disabled;
    }

    public void Dispose()
    {
        _limitTriggered.OnCompleted();
        _limitTriggered.Dispose();
        _limitWarning.OnCompleted();
        _limitWarning.Dispose();
    }
}
