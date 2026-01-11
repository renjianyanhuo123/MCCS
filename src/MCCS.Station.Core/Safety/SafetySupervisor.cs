using System.Reactive.Linq;
using System.Reactive.Subjects;
using MCCS.Station.Abstractions.Enums;
using MCCS.Station.Abstractions.Events;
using MCCS.Station.Abstractions.Interfaces;
using MCCS.Station.Abstractions.Models;

namespace MCCS.Station.Core.Safety;

/// <summary>
/// 安全主管
/// 整合 LimitEngine、InterlockEngine、EStopMonitor
/// 独立于状态机，对状态机有"硬打断/降级/锁定"的权限
/// </summary>
public sealed class SafetySupervisor : ISafetySupervisor, IDisposable
{
    private readonly object _lock = new();
    private readonly Subject<SafetyStatusChangedEvent> _safetyStatusChanged = new();
    private readonly IStatusAggregator _statusAggregator;
    private readonly IDisposable _subscriptions;

    private SafetyStatus _currentSafetyStatus = SafetyStatus.Normal;
    private volatile bool _isRunning;
    private CancellationTokenSource? _cts;

    public SafetySupervisor(
        IStatusAggregator statusAggregator,
        ILimitEngine? limitEngine = null,
        IInterlockEngine? interlockEngine = null,
        IEStopMonitor? estopMonitor = null)
    {
        _statusAggregator = statusAggregator ?? throw new ArgumentNullException(nameof(statusAggregator));

        // 使用提供的引擎或创建默认实例
        LimitEngine = limitEngine ?? new LimitEngine();
        InterlockEngine = interlockEngine ?? new InterlockEngine();
        EStopMonitor = estopMonitor ?? new EStopMonitor();

        // 订阅各个子系统的事件
        _subscriptions = SetupSubscriptions();
    }

    public SafetyStatus CurrentSafetyStatus
    {
        get
        {
            lock (_lock)
            {
                return _currentSafetyStatus;
            }
        }
    }

    public IObservable<SafetyStatusChangedEvent> SafetyStatusChanged => _safetyStatusChanged;

    public ILimitEngine LimitEngine { get; }
    public IInterlockEngine InterlockEngine { get; }
    public IEStopMonitor EStopMonitor { get; }

    public bool IsRunning => _isRunning;

    private IDisposable SetupSubscriptions()
    {
        var subscriptions = new List<IDisposable>
        {
            // 订阅限位事件
            LimitEngine.LimitTriggered.Subscribe(OnLimitTriggered),
            LimitEngine.LimitWarning.Subscribe(OnLimitWarning),
            // 订阅联锁事件
            InterlockEngine.InterlockTriggered.Subscribe(OnInterlockTriggered),
            // 订阅急停事件
            EStopMonitor.EStopTriggered.Subscribe(OnEStopTriggered)
        };

        return new CompositeDisposable(subscriptions);
    }

    private void OnLimitTriggered(SoftLimitTriggeredEvent evt)
    {
        RecalculateSafetyStatus(
            evt.IsTripped ? SafetyTriggerReason.SoftLimitTripped : SafetyTriggerReason.ConditionRestored,
            evt.IsTripped ? $"软限位触发: {evt.LimitId}" : $"软限位解除: {evt.LimitId}",
            [evt.ChannelId]);
    }

    private void OnLimitWarning(LimitTrippedEvent evt)
    {
        // 警告不改变安全状态，但添加问题
        _statusAggregator.AddIssue(new StatusIssue
        {
            Id = $"limit_warning_{evt.ChannelId}_{evt.SignalName}",
            Source = evt.ChannelId,
            Level = SafetyStatus.Warning,
            Message = $"接近限位: {evt.SignalName} = {evt.ActualValue:F2} (阈值: {evt.Threshold:F2})",
            OccurredAt = evt.Timestamp,
            AutoRecoverable = true,
            RecoveryHint = "值已接近限位，请注意"
        });
    }

    private void OnInterlockTriggered(InterlockTriggeredEvent evt)
    {
        if (evt.IsTripped)
        {
            _statusAggregator.AddIssue(new StatusIssue
            {
                Id = $"interlock_{evt.RuleId}",
                Source = "InterlockEngine",
                Level = SafetyStatus.Interlocked,
                Message = evt.Reason,
                OccurredAt = evt.Timestamp,
                AutoRecoverable = evt.ResetPolicy == InterlockResetPolicy.Auto,
                RecoveryHint = evt.ClearInstructions
            });
        }
        else
        {
            _statusAggregator.RemoveIssue($"interlock_{evt.RuleId}");
        }

        RecalculateSafetyStatus(
            evt.IsTripped ? SafetyTriggerReason.InterlockTripped : SafetyTriggerReason.ManualClear,
            evt.Reason,
            Array.Empty<string>());
    }

    private void OnEStopTriggered(EStopEvent evt)
    {
        if (evt.IsActivated)
        {
            _statusAggregator.AddIssue(new StatusIssue
            {
                Id = $"estop_{evt.Type}_{evt.TriggerSource}",
                Source = "EStopMonitor",
                Level = evt.Type == EStopType.Hardware ? SafetyStatus.EStop : SafetyStatus.Failsafe,
                Message = $"急停触发: {evt.TriggerSource}",
                OccurredAt = evt.Timestamp,
                AutoRecoverable = false,
                RecoveryHint = evt.ResetInstructions
            });
        }
        else
        {
            // 清除急停问题
            var issuePrefix = $"estop_{evt.Type}";
            foreach (var issue in _statusAggregator.GetActiveIssues().Where(i => i.Id.StartsWith(issuePrefix)))
            {
                _statusAggregator.RemoveIssue(issue.Id);
            }
        }

        RecalculateSafetyStatus(
            evt.IsActivated ? SafetyTriggerReason.EStopTripped : SafetyTriggerReason.Reset,
            evt.IsActivated ? $"急停触发: {evt.TriggerSource}" : "急停已释放",
            Array.Empty<string>());
    }

    private void RecalculateSafetyStatus(SafetyTriggerReason triggerReason, string description, IReadOnlyList<string> affectedChannels)
    {
        lock (_lock)
        {
            var previousStatus = _currentSafetyStatus;
            var newStatus = CalculateOverallSafetyStatus();

            if (newStatus != previousStatus)
            {
                _currentSafetyStatus = newStatus;

                // 更新状态聚合器
                _statusAggregator.UpdateSafety(newStatus, description, true);

                // 收集触发的规则
                var triggeredRules = new List<string>();
                foreach (var limit in LimitEngine.GetTrippedLimits())
                {
                    triggeredRules.Add($"Limit:{limit.Config.LimitId}");
                }
                foreach (var interlock in InterlockEngine.GetTrippedInterlocks())
                {
                    triggeredRules.Add($"Interlock:{interlock.Rule.RuleId}");
                }
                if (EStopMonitor.IsAnyEStopActive)
                {
                    //foreach (var source in EStopMonitor.GetActiveEStopSources())
                    //{
                    //    triggeredRules.Add($"EStop:{source.SourceId}");
                    //}
                }

                // 发布事件
                var evt = new SafetyStatusChangedEvent
                {
                    PreviousStatus = previousStatus,
                    CurrentStatus = newStatus,
                    TriggerReason = triggerReason,
                    TriggeredRules = triggeredRules.AsReadOnly(),
                    Description = description,
                    AffectedChannels = affectedChannels,
                    ActionTaken = GetActionTaken(newStatus),
                    RequiresIntervention = newStatus >= SafetyStatus.Interlocked,
                    Source = "SafetySupervisor"
                };

                Console.WriteLine($"[SafetySupervisor] 安全状态变更: {previousStatus} -> {newStatus} ({description})");
                _safetyStatusChanged.OnNext(evt);
            }
        }
    }

    private SafetyStatus CalculateOverallSafetyStatus()
    {
        // 优先级：EStop > Failsafe > Interlocked > Limited > Warning > Normal

        // 1. 检查急停
        if (EStopMonitor.IsHardwareEStopActive)
        {
            return SafetyStatus.EStop;
        }

        if (EStopMonitor.IsSoftwareEStopActive)
        {
            return SafetyStatus.Failsafe;
        }

        // 2. 检查联锁
        if (InterlockEngine.HasAnyTrippedInterlocks)
        {
            return SafetyStatus.Interlocked;
        }

        // 3. 检查软限位
        var trippedLimits = LimitEngine.GetTrippedLimits();
        if (trippedLimits.Count > 0)
        {
            // 检查是否有需要联锁的限位动作
            if (trippedLimits.Any(l => l.ActiveAction == LimitAction.TriggerInterlock))
            {
                return SafetyStatus.Interlocked;
            }
            if (trippedLimits.Any(l => l.ActiveAction == LimitAction.TriggerEStop))
            {
                return SafetyStatus.Failsafe;
            }
            return SafetyStatus.Limited;
        }

        // 4. 检查警告
        if (LimitEngine.HasAnyWarnings)
        {
            return SafetyStatus.Warning;
        }

        return SafetyStatus.Normal;
    }

    private static string GetActionTaken(SafetyStatus status)
    {
        return status switch
        {
            SafetyStatus.Normal => "无",
            SafetyStatus.Warning => "发出警告通知",
            SafetyStatus.Limited => "限制运动能力",
            SafetyStatus.Interlocked => "锁定控制操作，等待清除联锁",
            SafetyStatus.Failsafe => "切断能量链路，进入安全状态",
            SafetyStatus.EStop => "立即停止所有运动，切断能量，等待硬件复位",
            _ => "未知"
        };
    }

    public CapabilityFlags GetDisabledCapabilities()
    {
        var disabled = CapabilityFlags.None;

        disabled |= LimitEngine.GetDisabledCapabilities();
        disabled |= InterlockEngine.GetDisabledCapabilities();

        // 急停禁用所有能力
        if (EStopMonitor.IsAnyEStopActive)
        {
            disabled = CapabilityFlags.Full & ~CapabilityFlags.CanResetEStop;
        }

        return disabled;
    }

    public IReadOnlyList<StatusIssue> GetActiveSecurityIssues()
    {
        return _statusAggregator.GetActiveIssues()
            .Where(i => i.Level >= SafetyStatus.Warning)
            .ToList()
            .AsReadOnly();
    }

    public IReadOnlyList<InterlockState> GetTrippedInterlocks()
    {
        return InterlockEngine.GetTrippedInterlocks();
    }

    public IReadOnlyList<SoftLimitState> GetTrippedLimits()
    {
        return LimitEngine.GetTrippedLimits();
    }

    public async Task<bool> TryClearInterlockAsync(string ruleId, string operatorId, string reason)
    {
        var (success, message) = await InterlockEngine.TryClearInterlockAsync(ruleId, operatorId, reason);

        if (success)
        {
            Console.WriteLine($"[SafetySupervisor] 联锁清除成功: {ruleId}");
        }
        else
        {
            Console.WriteLine($"[SafetySupervisor] 联锁清除失败: {ruleId} - {message}");
        }

        return success;
    }

    public async Task<bool> TryResetEStopAsync(string operatorId, string reason)
    {
        var (success, message) = await EStopMonitor.TryResetAsync(operatorId, reason);

        if (success)
        {
            Console.WriteLine($"[SafetySupervisor] 急停复位成功");
            RecalculateSafetyStatus(SafetyTriggerReason.Reset, "急停已复位", Array.Empty<string>());
        }
        else
        {
            Console.WriteLine($"[SafetySupervisor] 急停复位失败: {message}");
        }

        return success;
    }

    public async Task TriggerSoftwareEStopAsync(string reason, string triggeredBy)
    {
        await EStopMonitor.TriggerSoftwareEStopAsync(reason, triggeredBy);
    }

    public async Task<bool> TryReleaseSoftwareEStopAsync(string operatorId, string reason)
    {
        var (success, message) = await EStopMonitor.TryReleaseSoftwareEStopAsync(operatorId, reason);
        return success;
    }

    public (bool Allowed, string Reason, IReadOnlyList<string> BlockingRules) CheckOperation(CapabilityFlags requiredCapabilities)
    {
        var blockingRules = new List<string>();

        // 检查急停
        //if (EStopMonitor.IsAnyEStopActive)
        //{
        //    var sources = EStopMonitor.GetActiveEStopSources();
        //    blockingRules.AddRange(sources.Select(s => $"EStop:{s.SourceId}"));
        //    return (false, "急停已激活，禁止所有操作", blockingRules.AsReadOnly());
        //}

        //// 检查联锁
        //var (interlockBlocked, interlockRules) = InterlockEngine.IsOperationBlocked(requiredCapabilities);
        //if (interlockBlocked)
        //{
        //    blockingRules.AddRange(interlockRules.Select(r => $"Interlock:{r}"));
        //    return (false, "操作被联锁阻止", blockingRules.AsReadOnly());
        //}

        //// 检查软限位禁用的能力
        //var limitDisabled = LimitEngine.GetDisabledCapabilities();
        //if ((limitDisabled & requiredCapabilities) != CapabilityFlags.None)
        //{
        //    foreach (var limit in LimitEngine.GetTrippedLimits())
        //    {
        //        blockingRules.Add($"Limit:{limit.Config.LimitId}");
        //    }
        //    return (false, "操作被软限位限制", blockingRules.AsReadOnly());
        //}

        return (true, string.Empty, blockingRules.AsReadOnly());
    }

    public async Task StartAsync(CancellationToken cancellationToken = default)
    {
        if (_isRunning)
        {
            return;
        }

        _cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        _isRunning = true;

        Console.WriteLine("[SafetySupervisor] 安全主管已启动");
        await Task.CompletedTask;
    }

    public Task StopAsync()
    {
        if (!_isRunning)
        {
            return Task.CompletedTask;
        }

        _cts?.Cancel();
        _isRunning = false;

        Console.WriteLine("[SafetySupervisor] 安全主管已停止");
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _subscriptions.Dispose();
        _safetyStatusChanged.OnCompleted();
        _safetyStatusChanged.Dispose();

        if (LimitEngine is IDisposable limitDisposable)
        {
            limitDisposable.Dispose();
        }

        if (InterlockEngine is IDisposable interlockDisposable)
        {
            interlockDisposable.Dispose();
        }

        if (EStopMonitor is IDisposable estopDisposable)
        {
            estopDisposable.Dispose();
        }

        _cts?.Dispose();
    }
}

internal sealed class CompositeDisposable : IDisposable
{
    private readonly List<IDisposable> _disposables;

    public CompositeDisposable(IEnumerable<IDisposable> disposables)
    {
        _disposables = disposables.ToList();
    }

    public void Dispose()
    {
        foreach (var disposable in _disposables)
        {
            disposable.Dispose();
        }
        _disposables.Clear();
    }
}
