using System.Collections.Concurrent;
using System.Reactive.Subjects;
using MCCS.Station.Abstractions.Events;
using MCCS.Station.Abstractions.Interfaces;

namespace MCCS.Station.Core.Safety;

/// <summary>
/// 急停监控器
/// 负责监控急停状态（硬件急停和软件急停）
/// 急停是最高优先级的安全措施，触发后必须人工干预
/// </summary>
public sealed class EStopMonitor : IEStopMonitor, IDisposable
{
    private readonly object _lock = new();
    private readonly Subject<EStopEvent> _estopTriggered = new();
    private readonly ConcurrentDictionary<string, EStopSource> _activeSources = new();

    private volatile bool _isHardwareEStopActive;
    private volatile bool _isSoftwareEStopActive;
    private string? _lastHardwareSource;

    public IObservable<EStopEvent> EStopTriggered => _estopTriggered;

    public bool IsHardwareEStopActive
    {
        get
        {
            lock (_lock)
            {
                return _isHardwareEStopActive;
            }
        }
    }

    public bool IsSoftwareEStopActive
    {
        get
        {
            lock (_lock)
            {
                return _isSoftwareEStopActive;
            }
        }
    }

    public DateTime? LastEStopTime { get; private set; }

    public string? LastEStopReason { get; private set; }

    public bool RequiresHardwareReset
    {
        get
        {
            lock (_lock)
            {
                return _isHardwareEStopActive ||
                       _activeSources.Values.Any(s => s.Type == EStopType.Hardware);
            }
        }
    }

    public IReadOnlyList<EStopSource> GetActiveEStopSources()
    {
        return _activeSources.Values.ToList().AsReadOnly();
    }

    public void UpdateHardwareEStopState(bool isActive, string source)
    {
        lock (_lock)
        {
            var wasActive = _isHardwareEStopActive;
            _isHardwareEStopActive = isActive;
            _lastHardwareSource = source;

            if (isActive && !wasActive)
            {
                // 硬件急停激活
                var now = DateTime.UtcNow;
                LastEStopTime = now;
                LastEStopReason = $"硬件急停触发: {source}";

                var estopSource = new EStopSource
                {
                    SourceId = $"hw_{source}",
                    Type = EStopType.Hardware,
                    TriggeredAt = now,
                    Reason = $"硬件急停按钮: {source}",
                    TriggeredBy = "Hardware",
                    CanSoftwareRelease = false
                };

                _activeSources[$"hw_{source}"] = estopSource;

                var evt = new EStopEvent
                {
                    Type = EStopType.Hardware,
                    IsActivated = true,
                    TriggerSource = source,
                    ActionsTaken = new[] { "立即切断能量链路", "停止所有运动", "关闭阀台" },
                    RequiresHardwareReset = true,
                    ResetInstructions = "1. 确认现场安全\n2. 复位急停按钮\n3. 点击"急停复位"按钮",
                    Source = "EStopMonitor"
                };

                Console.WriteLine($"[EStopMonitor] 硬件急停激活: {source}");
                _estopTriggered.OnNext(evt);
            }
            else if (!isActive && wasActive)
            {
                // 硬件急停释放
                _activeSources.TryRemove($"hw_{source}", out _);

                // 检查是否所有硬件急停都已释放
                if (!_activeSources.Values.Any(s => s.Type == EStopType.Hardware))
                {
                    var evt = new EStopEvent
                    {
                        Type = EStopType.Hardware,
                        IsActivated = false,
                        TriggerSource = source,
                        ActionsTaken = new[] { "硬件急停已释放" },
                        RequiresHardwareReset = false,
                        ResetInstructions = "硬件急停已释放，请点击"急停复位"确认恢复",
                        Source = "EStopMonitor"
                    };

                    Console.WriteLine($"[EStopMonitor] 硬件急停释放: {source}");
                    _estopTriggered.OnNext(evt);
                }
            }
        }
    }

    public async Task TriggerSoftwareEStopAsync(string reason, string triggeredBy)
    {
        lock (_lock)
        {
            if (_isSoftwareEStopActive)
            {
                Console.WriteLine($"[EStopMonitor] 软件急停已激活，忽略重复触发");
                return;
            }

            _isSoftwareEStopActive = true;

            var now = DateTime.UtcNow;
            LastEStopTime = now;
            LastEStopReason = reason;

            var sourceId = $"sw_{triggeredBy}_{now.Ticks}";
            var estopSource = new EStopSource
            {
                SourceId = sourceId,
                Type = EStopType.Software,
                TriggeredAt = now,
                Reason = reason,
                TriggeredBy = triggeredBy,
                CanSoftwareRelease = true
            };

            _activeSources[sourceId] = estopSource;

            var evt = new EStopEvent
            {
                Type = EStopType.Software,
                IsActivated = true,
                TriggerSource = triggeredBy,
                ActionsTaken = new[] { "停止当前试验", "进入安全状态", "等待操作员确认" },
                RequiresHardwareReset = false,
                ResetInstructions = $"软件急停由 {triggeredBy} 触发\n原因: {reason}\n\n请确认安全后点击"释放软件急停"",
                Source = "EStopMonitor"
            };

            Console.WriteLine($"[EStopMonitor] 软件急停触发: {reason} (由 {triggeredBy})");
            _estopTriggered.OnNext(evt);
        }

        await Task.CompletedTask;
    }

    public async Task<(bool Success, string Message)> TryReleaseSoftwareEStopAsync(string operatorId, string reason)
    {
        lock (_lock)
        {
            if (!_isSoftwareEStopActive)
            {
                return (false, "软件急停未激活");
            }

            // 检查是否有硬件急停
            if (_isHardwareEStopActive)
            {
                return (false, "硬件急停仍然激活，请先复位硬件急停");
            }

            _isSoftwareEStopActive = false;

            // 移除所有软件急停源
            var swSources = _activeSources.Keys.Where(k => k.StartsWith("sw_")).ToList();
            foreach (var key in swSources)
            {
                _activeSources.TryRemove(key, out _);
            }

            var evt = new EStopEvent
            {
                Type = EStopType.Software,
                IsActivated = false,
                TriggerSource = operatorId,
                ActionsTaken = new[] { $"软件急停由 {operatorId} 释放", $"原因: {reason}" },
                RequiresHardwareReset = false,
                ResetInstructions = string.Empty,
                Source = "EStopMonitor"
            };

            Console.WriteLine($"[EStopMonitor] 软件急停释放: 由 {operatorId}, 原因: {reason}");
            _estopTriggered.OnNext(evt);
        }

        await Task.CompletedTask;
        return (true, "软件急停已成功释放");
    }

    public async Task<(bool Success, string Message)> TryResetAsync(string operatorId, string reason)
    {
        lock (_lock)
        {
            // 检查硬件急停状态
            if (_isHardwareEStopActive)
            {
                return (false, $"硬件急停仍然激活（来源: {_lastHardwareSource}）。请先复位物理急停按钮。");
            }

            // 检查是否有不可软件释放的急停源
            var hardSources = _activeSources.Values.Where(s => !s.CanSoftwareRelease).ToList();
            if (hardSources.Count > 0)
            {
                var sourceList = string.Join(", ", hardSources.Select(s => s.SourceId));
                return (false, $"以下急停源需要硬件复位: {sourceList}");
            }

            // 清除所有可软件释放的急停源
            var softSources = _activeSources.Keys.Where(k =>
                _activeSources.TryGetValue(k, out var s) && s.CanSoftwareRelease).ToList();

            foreach (var key in softSources)
            {
                _activeSources.TryRemove(key, out _);
            }

            _isSoftwareEStopActive = false;

            if (_activeSources.IsEmpty)
            {
                LastEStopTime = null;
                LastEStopReason = null;
            }

            var evt = new EStopEvent
            {
                Type = EStopType.Software,
                IsActivated = false,
                TriggerSource = operatorId,
                ActionsTaken = new[]
                {
                    "急停系统复位",
                    $"操作员: {operatorId}",
                    $"原因: {reason}",
                    "系统准备恢复正常运行"
                },
                RequiresHardwareReset = false,
                ResetInstructions = string.Empty,
                Source = "EStopMonitor"
            };

            Console.WriteLine($"[EStopMonitor] 急停系统复位: 由 {operatorId}, 原因: {reason}");
            _estopTriggered.OnNext(evt);
        }

        await Task.CompletedTask;
        return (true, "急停系统已成功复位");
    }

    public string GetResetInstructions()
    {
        lock (_lock)
        {
            if (!_isHardwareEStopActive && !_isSoftwareEStopActive)
            {
                return "急停未激活";
            }

            var instructions = new List<string>();

            if (_isHardwareEStopActive)
            {
                instructions.Add("1. 确认现场已安全");
                instructions.Add($"2. 复位硬件急停按钮（来源: {_lastHardwareSource ?? "未知"}）");
                instructions.Add("3. 确认急停指示灯已熄灭");
            }

            if (_isSoftwareEStopActive)
            {
                var swSources = _activeSources.Values.Where(s => s.Type == EStopType.Software).ToList();
                if (swSources.Count > 0)
                {
                    var lastSource = swSources.OrderByDescending(s => s.TriggeredAt).First();
                    instructions.Add($"软件急停触发原因: {lastSource.Reason}");
                    instructions.Add($"触发者: {lastSource.TriggeredBy}");
                }
            }

            instructions.Add("");
            instructions.Add("完成上述步骤后，点击"急停复位"按钮确认恢复");

            return string.Join("\n", instructions);
        }
    }

    public void Dispose()
    {
        _estopTriggered.OnCompleted();
        _estopTriggered.Dispose();
    }
}
