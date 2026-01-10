using System.Reactive.Subjects;
using MCCS.Station.Abstractions.Enums;
using MCCS.Station.Abstractions.Events;
using MCCS.Station.Abstractions.Interfaces;
using MCCS.Station.Abstractions.Models;

namespace MCCS.Station.Core.Safety;

/// <summary>
/// 状态合成器
/// 负责将四个正交维度合成为站点总状态
/// 按优先级从高到低覆盖：EStop > Interlocked > Limited > Degraded > 流程状态
/// </summary>
public sealed class StatusAggregator : IStatusAggregator, IDisposable
{
    private readonly object _lock = new();
    private readonly Subject<CompositeStatusChangedEvent> _statusChanged = new();
    private readonly List<StatusIssue> _activeIssues = new();

    private ConnectivityStatus _connectivity = ConnectivityStatus.Disconnected;
    private ActivationStatus _activation = ActivationStatus.Off;
    private ProcessStatus _process = ProcessStatus.Idle;
    private SafetyStatus _safety = SafetyStatus.Normal;
    private CapabilityFlags _capabilities = CapabilityFlags.None;

    private StationCompositeStatus _currentStatus;

    public StatusAggregator()
    {
        _currentStatus = BuildStatus();
    }

    public StationCompositeStatus CurrentStatus
    {
        get { lock (_lock) return _currentStatus; }
    }

    public IObservable<CompositeStatusChangedEvent> StatusChanged => _statusChanged;

    public void UpdateConnectivity(ConnectivityStatus status, string reason = "")
    {
        lock (_lock)
        {
            if (_connectivity == status) return;

            var previous = _currentStatus;
            _connectivity = status;
            _capabilities = RecalculateCapabilitiesInternal();
            _currentStatus = BuildStatus();

            PublishChange(previous, StatusDimension.Connectivity, reason, false);
        }
    }

    public void UpdateActivation(ActivationStatus status, string reason = "")
    {
        lock (_lock)
        {
            if (_activation == status) return;

            var previous = _currentStatus;
            _activation = status;
            _capabilities = RecalculateCapabilitiesInternal();
            _currentStatus = BuildStatus();

            PublishChange(previous, StatusDimension.Activation, reason, false);
        }
    }

    public void UpdateProcess(ProcessStatus status, string reason = "")
    {
        lock (_lock)
        {
            if (_process == status) return;

            var previous = _currentStatus;
            _process = status;
            _capabilities = RecalculateCapabilitiesInternal();
            _currentStatus = BuildStatus();

            PublishChange(previous, StatusDimension.Process, reason, false);
        }
    }

    public void UpdateSafety(SafetyStatus status, string reason = "", bool triggeredBySafety = true)
    {
        lock (_lock)
        {
            if (_safety == status) return;

            var previous = _currentStatus;
            _safety = status;
            _capabilities = RecalculateCapabilitiesInternal();
            _currentStatus = BuildStatus();

            PublishChange(previous, StatusDimension.Safety, reason, triggeredBySafety);
        }
    }

    public void AddIssue(StatusIssue issue)
    {
        lock (_lock)
        {
            // 检查是否已存在
            var existingIndex = _activeIssues.FindIndex(i => i.Id == issue.Id);
            if (existingIndex >= 0)
            {
                _activeIssues[existingIndex] = issue;
            }
            else
            {
                _activeIssues.Add(issue);
            }

            // 根据问题级别更新安全状态
            var maxLevel = _activeIssues.Count > 0
                ? _activeIssues.Max(i => i.Level)
                : SafetyStatus.Normal;

            if (maxLevel > _safety)
            {
                UpdateSafety(maxLevel, $"问题触发: {issue.Message}", true);
            }
            else
            {
                // 仅更新问题列表，不改变状态
                _currentStatus = BuildStatus();
            }
        }
    }

    public void RemoveIssue(string issueId)
    {
        lock (_lock)
        {
            var removed = _activeIssues.RemoveAll(i => i.Id == issueId);
            if (removed > 0)
            {
                // 重新计算安全状态
                var maxLevel = _activeIssues.Count > 0
                    ? _activeIssues.Max(i => i.Level)
                    : SafetyStatus.Normal;

                if (maxLevel < _safety)
                {
                    UpdateSafety(maxLevel, "问题已清除", true);
                }
                else
                {
                    _currentStatus = BuildStatus();
                }
            }
        }
    }

    public void ClearAutoRecoverableIssues()
    {
        lock (_lock)
        {
            var removed = _activeIssues.RemoveAll(i => i.AutoRecoverable);
            if (removed > 0)
            {
                var maxLevel = _activeIssues.Count > 0
                    ? _activeIssues.Max(i => i.Level)
                    : SafetyStatus.Normal;

                if (maxLevel < _safety)
                {
                    UpdateSafety(maxLevel, "可自动恢复的问题已清除", true);
                }
                else
                {
                    _currentStatus = BuildStatus();
                }
            }
        }
    }

    public CapabilityFlags RecalculateCapabilities()
    {
        lock (_lock)
        {
            var previous = _currentStatus;
            _capabilities = RecalculateCapabilitiesInternal();
            _currentStatus = BuildStatus();

            if (previous.Capabilities != _capabilities)
            {
                PublishChange(previous, StatusDimension.Capabilities, "能力重新计算", false);
            }

            return _capabilities;
        }
    }

    private CapabilityFlags RecalculateCapabilitiesInternal()
    {
        var caps = CapabilityFlags.None;

        // 根据连接状态
        switch (_connectivity)
        {
            case ConnectivityStatus.Disconnected:
                caps |= CapabilityFlags.CanConnect;
                return caps; // 断开状态下只能连接

            case ConnectivityStatus.Connecting:
                return CapabilityFlags.ReadOnly; // 连接中只能观察

            case ConnectivityStatus.Degraded:
                caps |= CapabilityFlags.CanConnect | CapabilityFlags.ReadOnly;
                break;

            case ConnectivityStatus.Ready:
                caps |= CapabilityFlags.CanConnect | CapabilityFlags.CanActivate |
                        CapabilityFlags.CanTare | CapabilityFlags.CanCalibrate;
                break;
        }

        // 根据激活状态
        switch (_activation)
        {
            case ActivationStatus.Off:
                caps |= CapabilityFlags.CanActivate;
                break;

            case ActivationStatus.Activating:
            case ActivationStatus.Deactivating:
                // 过渡态，限制操作
                break;

            case ActivationStatus.Low:
                caps |= CapabilityFlags.CanPressurize | CapabilityFlags.CanManualControl;
                break;

            case ActivationStatus.Pressurizing:
            case ActivationStatus.Depressurizing:
                // 过渡态
                break;

            case ActivationStatus.High:
                caps |= CapabilityFlags.CanMove | CapabilityFlags.CanControl |
                        CapabilityFlags.CanManualControl | CapabilityFlags.CanRecord;
                break;
        }

        // 根据流程状态
        switch (_process)
        {
            case ProcessStatus.Idle:
            case ProcessStatus.Armed:
            case ProcessStatus.Completed:
                caps |= CapabilityFlags.CanStartTest | CapabilityFlags.CanModifyParameters;
                break;

            case ProcessStatus.Running:
                caps |= CapabilityFlags.CanPause | CapabilityFlags.CanStop;
                caps &= ~CapabilityFlags.CanModifyParameters;
                break;

            case ProcessStatus.Paused:
                caps |= CapabilityFlags.CanResume | CapabilityFlags.CanStop;
                break;

            case ProcessStatus.Manual:
                caps |= CapabilityFlags.CanManualControl | CapabilityFlags.CanStop;
                break;
        }

        // 根据安全状态削减能力
        switch (_safety)
        {
            case SafetyStatus.Normal:
                // 无削减
                break;

            case SafetyStatus.Warning:
                // 警告不削减能力，但应该注意
                break;

            case SafetyStatus.Limited:
                // 受限：禁止启动新试验，限制运动
                caps &= ~(CapabilityFlags.CanStartTest | CapabilityFlags.CanMove);
                break;

            case SafetyStatus.Interlocked:
                // 联锁：只允许清除联锁和基本监控
                caps = CapabilityFlags.CanClearInterlock | CapabilityFlags.ReadOnly;
                break;

            case SafetyStatus.Failsafe:
            case SafetyStatus.EStop:
                // 急停/失控保护：只允许复位和监控
                caps = CapabilityFlags.CanResetEStop | CapabilityFlags.ReadOnly;
                break;
        }

        return caps;
    }

    public bool HasCapability(CapabilityFlags capability)
    {
        lock (_lock)
        {
            return (_capabilities & capability) == capability;
        }
    }

    public bool CanExecuteCommand(CommandType commandType)
    {
        var required = GetRequiredCapabilitiesForCommand(commandType);
        return HasCapability(required);
    }

    private static CapabilityFlags GetRequiredCapabilitiesForCommand(CommandType commandType)
    {
        return commandType switch
        {
            CommandType.Connect or CommandType.Disconnect => CapabilityFlags.CanConnect,
            CommandType.Activate or CommandType.Deactivate => CapabilityFlags.CanActivate,
            CommandType.Pressurize or CommandType.Depressurize => CapabilityFlags.CanPressurize,
            CommandType.OpenValve or CommandType.CloseValve => CapabilityFlags.CanActivate,
            CommandType.StartControl or CommandType.StopControl => CapabilityFlags.CanControl,
            CommandType.SetControlMode or CommandType.SetSetpoint => CapabilityFlags.CanControl,
            CommandType.LoadTest => CapabilityFlags.CanModifyParameters,
            CommandType.StartTest => CapabilityFlags.CanStartTest,
            CommandType.PauseTest => CapabilityFlags.CanPause,
            CommandType.ResumeTest => CapabilityFlags.CanResume,
            CommandType.StopTest or CommandType.AbortTest => CapabilityFlags.CanStop,
            CommandType.ManualMove or CommandType.ManualStop or CommandType.Jog => CapabilityFlags.CanManualControl,
            CommandType.ClearInterlock => CapabilityFlags.CanClearInterlock,
            CommandType.ResetEStop or CommandType.SoftwareEStop or CommandType.ReleaseSoftwareEStop => CapabilityFlags.CanResetEStop,
            CommandType.Tare => CapabilityFlags.CanTare,
            CommandType.Calibrate => CapabilityFlags.CanCalibrate,
            CommandType.StartRecording or CommandType.StopRecording => CapabilityFlags.CanRecord,
            _ => CapabilityFlags.None
        };
    }

    public string GetStatusSummary()
    {
        lock (_lock)
        {
            return _currentStatus.Summary;
        }
    }

    public IReadOnlyList<StatusIssue> GetActiveIssues()
    {
        lock (_lock)
        {
            return _activeIssues.ToList().AsReadOnly();
        }
    }

    public Task RefreshAsync(CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            _capabilities = RecalculateCapabilitiesInternal();
            _currentStatus = BuildStatus();
        }
        return Task.CompletedTask;
    }

    private StationCompositeStatus BuildStatus()
    {
        return new StationCompositeStatus
        {
            Connectivity = _connectivity,
            Activation = _activation,
            Process = _process,
            Safety = _safety,
            Capabilities = _capabilities,
            ActiveIssues = _activeIssues.ToList().AsReadOnly(),
            UpdatedAt = DateTime.UtcNow,
            Summary = GenerateSummary()
        };
    }

    private string GenerateSummary()
    {
        // 安全状态优先级最高
        if (_safety == SafetyStatus.EStop)
            return "急停已触发";
        if (_safety == SafetyStatus.Failsafe)
            return "失控保护已激活";
        if (_safety == SafetyStatus.Interlocked)
            return $"联锁已触发 ({_activeIssues.Count}个问题)";
        if (_safety == SafetyStatus.Limited)
            return "受限运行中";

        // 连接状态
        if (_connectivity == ConnectivityStatus.Disconnected)
            return "未连接";
        if (_connectivity == ConnectivityStatus.Connecting)
            return "正在连接...";
        if (_connectivity == ConnectivityStatus.Degraded)
            return "资源降级";

        // 激活状态
        if (_activation == ActivationStatus.Off)
            return "就绪(能量关闭)";
        if (_activation < ActivationStatus.High)
            return $"就绪({_activation})";

        // 流程状态
        return _process switch
        {
            ProcessStatus.Idle => "就绪待机",
            ProcessStatus.Armed => "已准备",
            ProcessStatus.Preparing => "准备中...",
            ProcessStatus.Running => _safety == SafetyStatus.Warning ? "运行中(警告)" : "运行中",
            ProcessStatus.Paused => "已暂停",
            ProcessStatus.Stopping => "停止中...",
            ProcessStatus.Completed => "试验完成",
            ProcessStatus.Unloading => "回零中...",
            ProcessStatus.Manual => "手动模式",
            _ => "未知状态"
        };
    }

    private void PublishChange(StationCompositeStatus previous, StatusDimension dimension, string reason, bool triggeredBySafety)
    {
        var evt = new CompositeStatusChangedEvent
        {
            PreviousStatus = previous,
            CurrentStatus = _currentStatus,
            ChangedDimension = dimension,
            Reason = reason,
            TriggeredBySafety = triggeredBySafety,
            Source = "StatusAggregator"
        };

        Console.WriteLine($"[StatusAggregator] {previous} -> {_currentStatus} ({reason})");
        _statusChanged.OnNext(evt);
    }

    public void Dispose()
    {
        _statusChanged.OnCompleted();
        _statusChanged.Dispose();
    }
}
