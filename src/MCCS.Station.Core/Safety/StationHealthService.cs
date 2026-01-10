using System.Collections.Concurrent;
using System.Reactive.Subjects;
using MCCS.Station.Abstractions.Enums;
using MCCS.Station.Abstractions.Events;
using MCCS.Station.Abstractions.Interfaces;
using MCCS.Station.Abstractions.Models;

namespace MCCS.Station.Core.Safety;

/// <summary>
/// 站点健康服务
/// 负责周期性采样底层状态（来自采集进程/控制器）
/// 从资源树自底向上计算站点状态
/// </summary>
public sealed class StationHealthService : IStationHealthService, IDisposable
{
    private readonly object _lock = new();
    private readonly ConcurrentDictionary<string, ResourceHealthInfo> _resources = new();
    private readonly Subject<ResourceHealthChangedEvent> _resourceHealthChanged = new();
    private readonly IStatusAggregator? _statusAggregator;

    private volatile bool _isRunning;
    private CancellationTokenSource? _cts;
    private Task? _monitorTask;

    private ConnectivityStatus _overallConnectivity = ConnectivityStatus.Disconnected;
    private ActivationStatus _overallActivation = ActivationStatus.Off;

    public StationHealthService(IStatusAggregator? statusAggregator = null)
    {
        _statusAggregator = statusAggregator;
    }

    public IObservable<ResourceHealthChangedEvent> ResourceHealthChanged => _resourceHealthChanged;

    public ConnectivityStatus OverallConnectivity
    {
        get
        {
            lock (_lock)
            {
                return _overallConnectivity;
            }
        }
    }

    public ActivationStatus OverallActivation
    {
        get
        {
            lock (_lock)
            {
                return _overallActivation;
            }
        }
    }

    public int HealthCheckIntervalMs { get; set; } = 1000;

    public bool IsRunning => _isRunning;

    public void RegisterResource(ResourceHealthInfo resource)
    {
        if (string.IsNullOrEmpty(resource.ResourceId))
            throw new ArgumentException("ResourceId cannot be empty", nameof(resource));

        _resources[resource.ResourceId] = resource;
        Console.WriteLine($"[HealthService] 注册资源: {resource.ResourceId} ({resource.Name}) - 类型: {resource.Type}");
    }

    public void RegisterResources(IEnumerable<ResourceHealthInfo> resources)
    {
        foreach (var resource in resources)
        {
            RegisterResource(resource);
        }
    }

    public bool RemoveResource(string resourceId)
    {
        var removed = _resources.TryRemove(resourceId, out _);
        if (removed)
        {
            Console.WriteLine($"[HealthService] 移除资源: {resourceId}");
        }
        return removed;
    }

    public void UpdateResourceHealth(string resourceId, ResourceHealth health, FaultClass faultClass = FaultClass.None,
        string? details = null, string? faultCode = null)
    {
        if (!_resources.TryGetValue(resourceId, out var resource))
        {
            Console.WriteLine($"[HealthService] 资源不存在: {resourceId}");
            return;
        }

        var previousHealth = resource.Health;
        var now = DateTime.UtcNow;

        var updatedResource = new ResourceHealthInfo
        {
            ResourceId = resource.ResourceId,
            Name = resource.Name,
            Type = resource.Type,
            ParentId = resource.ParentId,
            Health = health,
            FaultClass = faultClass,
            Capabilities = CalculateResourceCapabilities(resource.Type, health),
            Details = details ?? resource.Details,
            FaultCode = faultCode,
            LastUpdated = now,
            LastOnline = health == ResourceHealth.Ok ? now : resource.LastOnline,
            Children = resource.Children,
            Diagnostics = resource.Diagnostics
        };

        _resources[resourceId] = updatedResource;

        if (previousHealth != health)
        {
            var evt = new ResourceHealthChangedEvent
            {
                ResourceId = resourceId,
                ResourceName = resource.Name,
                ResourceType = resource.Type,
                PreviousHealth = previousHealth,
                CurrentHealth = health,
                FaultClass = faultClass,
                FaultCode = faultCode,
                Details = details ?? string.Empty,
                Source = "StationHealthService"
            };

            Console.WriteLine($"[HealthService] 资源健康变更: {resource.Name} ({resourceId}): {previousHealth} -> {health}");
            _resourceHealthChanged.OnNext(evt);

            // 重新计算整体状态
            RecalculateOverallStatus();
        }
    }

    private static CapabilityFlags CalculateResourceCapabilities(ResourceType type, ResourceHealth health)
    {
        if (health == ResourceHealth.Fault)
        {
            return CapabilityFlags.ReadOnly;
        }

        return type switch
        {
            ResourceType.Controller => health == ResourceHealth.Ok
                ? CapabilityFlags.CanConnect | CapabilityFlags.CanActivate | CapabilityFlags.CanControl
                : CapabilityFlags.CanConnect,

            ResourceType.Valve => health == ResourceHealth.Ok
                ? CapabilityFlags.CanActivate | CapabilityFlags.CanPressurize
                : CapabilityFlags.None,

            ResourceType.HydraulicPowerUnit => health == ResourceHealth.Ok
                ? CapabilityFlags.CanActivate | CapabilityFlags.CanPressurize
                : CapabilityFlags.None,

            ResourceType.ControlChannel => health == ResourceHealth.Ok
                ? CapabilityFlags.CanControl | CapabilityFlags.CanMove | CapabilityFlags.CanStartTest
                : CapabilityFlags.ReadOnly,

            ResourceType.Signal => health == ResourceHealth.Ok
                ? CapabilityFlags.CanRecord | CapabilityFlags.CanTare
                : CapabilityFlags.ReadOnly,

            ResourceType.Sensor => health == ResourceHealth.Ok
                ? CapabilityFlags.CanCalibrate | CapabilityFlags.CanTare
                : CapabilityFlags.ReadOnly,

            _ => health == ResourceHealth.Ok ? CapabilityFlags.Full : CapabilityFlags.ReadOnly
        };
    }

    public IReadOnlyList<ResourceHealthInfo> GetAllResources()
    {
        return _resources.Values.ToList().AsReadOnly();
    }

    public IReadOnlyList<ResourceHealthInfo> GetResourcesByType(ResourceType type)
    {
        return _resources.Values
            .Where(r => r.Type == type)
            .ToList()
            .AsReadOnly();
    }

    public ResourceHealthInfo? GetResource(string resourceId)
    {
        return _resources.TryGetValue(resourceId, out var resource) ? resource : null;
    }

    public IReadOnlyList<ResourceHealthInfo> GetResourceTree()
    {
        // 获取根资源（没有父节点的资源）
        var roots = _resources.Values
            .Where(r => string.IsNullOrEmpty(r.ParentId))
            .Select(r => BuildResourceNode(r))
            .ToList();

        return roots.AsReadOnly();
    }

    private ResourceHealthInfo BuildResourceNode(ResourceHealthInfo resource)
    {
        var children = _resources.Values
            .Where(r => r.ParentId == resource.ResourceId)
            .Select(r => BuildResourceNode(r))
            .ToList();

        if (children.Count == 0)
        {
            return resource;
        }

        return new ResourceHealthInfo
        {
            ResourceId = resource.ResourceId,
            Name = resource.Name,
            Type = resource.Type,
            ParentId = resource.ParentId,
            Health = resource.Health,
            FaultClass = resource.FaultClass,
            Capabilities = resource.Capabilities,
            Details = resource.Details,
            FaultCode = resource.FaultCode,
            LastUpdated = resource.LastUpdated,
            LastOnline = resource.LastOnline,
            Children = children.AsReadOnly(),
            Diagnostics = resource.Diagnostics
        };
    }

    public IReadOnlyList<ResourceHealthInfo> GetFaultedResources()
    {
        return _resources.Values
            .Where(r => r.Health == ResourceHealth.Fault)
            .ToList()
            .AsReadOnly();
    }

    public IReadOnlyList<ResourceHealthInfo> GetWarningResources()
    {
        return _resources.Values
            .Where(r => r.Health == ResourceHealth.Warning)
            .ToList()
            .AsReadOnly();
    }

    public ConnectivityStatus CalculateConnectivityStatus()
    {
        var resources = _resources.Values.ToList();

        if (resources.Count == 0)
        {
            return ConnectivityStatus.Disconnected;
        }

        // 检查关键资源（控制器）
        var controllers = resources.Where(r => r.Type == ResourceType.Controller).ToList();
        if (controllers.Count == 0 || controllers.All(c => c.Health == ResourceHealth.Fault || c.Health == ResourceHealth.Unknown))
        {
            return ConnectivityStatus.Disconnected;
        }

        // 如果有控制器正在连接中
        if (resources.Any(r => r.Details?.Contains("Connecting") == true))
        {
            return ConnectivityStatus.Connecting;
        }

        // 检查是否有故障或警告
        var hasFault = resources.Any(r => r.Health == ResourceHealth.Fault);
        var hasWarning = resources.Any(r => r.Health == ResourceHealth.Warning);

        if (hasFault)
        {
            return ConnectivityStatus.Degraded;
        }

        if (hasWarning)
        {
            return ConnectivityStatus.Degraded;
        }

        return ConnectivityStatus.Ready;
    }

    public ActivationStatus CalculateActivationStatus()
    {
        var resources = _resources.Values.ToList();

        // 检查油源/液压单元
        var hpu = resources.Where(r => r.Type == ResourceType.HydraulicPowerUnit).ToList();
        var valves = resources.Where(r => r.Type == ResourceType.Valve).ToList();

        // 如果没有能量相关资源，默认为Off
        if (hpu.Count == 0 && valves.Count == 0)
        {
            return ActivationStatus.Off;
        }

        // 检查诊断数据中的状态
        foreach (var resource in hpu.Concat(valves))
        {
            if (resource.Diagnostics != null)
            {
                if (resource.Diagnostics.TryGetValue("activation_status", out var status))
                {
                    if (status is ActivationStatus activationStatus)
                    {
                        return activationStatus;
                    }
                    if (status is string statusStr && Enum.TryParse<ActivationStatus>(statusStr, out var parsed))
                    {
                        return parsed;
                    }
                }
            }
        }

        // 基于健康状态推断
        if (hpu.Any(h => h.Health == ResourceHealth.Ok) || valves.Any(v => v.Health == ResourceHealth.Ok))
        {
            return ActivationStatus.Low; // 至少处于低压状态
        }

        return ActivationStatus.Off;
    }

    public CapabilityFlags CalculateCapabilities()
    {
        var caps = CapabilityFlags.None;

        foreach (var resource in _resources.Values.Where(r => r.Health == ResourceHealth.Ok))
        {
            caps |= resource.Capabilities;
        }

        return caps;
    }

    private void RecalculateOverallStatus()
    {
        lock (_lock)
        {
            var newConnectivity = CalculateConnectivityStatus();
            var newActivation = CalculateActivationStatus();

            if (newConnectivity != _overallConnectivity)
            {
                _overallConnectivity = newConnectivity;
                _statusAggregator?.UpdateConnectivity(newConnectivity, "资源状态变化");
            }

            if (newActivation != _overallActivation)
            {
                _overallActivation = newActivation;
                _statusAggregator?.UpdateActivation(newActivation, "能量状态变化");
            }
        }
    }

    public async Task<HealthCheckResult> PerformHealthCheckAsync(CancellationToken cancellationToken = default)
    {
        var sw = System.Diagnostics.Stopwatch.StartNew();
        var issues = new List<StatusIssue>();

        var resources = _resources.Values.ToList();
        var healthyCount = resources.Count(r => r.Health == ResourceHealth.Ok);
        var warningCount = resources.Count(r => r.Health == ResourceHealth.Warning);
        var faultedCount = resources.Count(r => r.Health == ResourceHealth.Fault);

        foreach (var resource in resources.Where(r => r.Health == ResourceHealth.Fault))
        {
            issues.Add(new StatusIssue
            {
                Id = $"health_fault_{resource.ResourceId}",
                Source = resource.ResourceId,
                Level = resource.FaultClass switch
                {
                    FaultClass.Failsafe => SafetyStatus.Failsafe,
                    FaultClass.Interlock => SafetyStatus.Interlocked,
                    _ => SafetyStatus.Warning
                },
                Message = $"资源故障: {resource.Name} - {resource.Details}",
                OccurredAt = resource.LastUpdated,
                AutoRecoverable = resource.FaultClass == FaultClass.Recoverable,
                RecoveryHint = resource.FaultCode ?? "检查资源状态"
            });
        }

        foreach (var resource in resources.Where(r => r.Health == ResourceHealth.Warning))
        {
            issues.Add(new StatusIssue
            {
                Id = $"health_warning_{resource.ResourceId}",
                Source = resource.ResourceId,
                Level = SafetyStatus.Warning,
                Message = $"资源警告: {resource.Name} - {resource.Details}",
                OccurredAt = resource.LastUpdated,
                AutoRecoverable = true,
                RecoveryHint = "监控资源状态"
            });
        }

        sw.Stop();

        var connectivity = CalculateConnectivityStatus();
        var activation = CalculateActivationStatus();
        var capabilities = CalculateCapabilities();

        return new HealthCheckResult
        {
            IsHealthy = faultedCount == 0,
            Connectivity = connectivity,
            Activation = activation,
            Capabilities = capabilities,
            TotalResources = resources.Count,
            HealthyResources = healthyCount,
            WarningResources = warningCount,
            FaultedResources = faultedCount,
            Issues = issues.AsReadOnly(),
            CheckedAt = DateTime.UtcNow,
            DurationMs = (int)sw.ElapsedMilliseconds
        };
    }

    public async Task StartAsync(CancellationToken cancellationToken = default)
    {
        if (_isRunning)
        {
            return;
        }

        _cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        _isRunning = true;

        _monitorTask = Task.Run(async () =>
        {
            while (!_cts.Token.IsCancellationRequested)
            {
                try
                {
                    await Task.Delay(HealthCheckIntervalMs, _cts.Token);

                    // 执行周期性健康检查
                    var result = await PerformHealthCheckAsync(_cts.Token);

                    // 更新问题列表
                    if (_statusAggregator != null)
                    {
                        _statusAggregator.ClearAutoRecoverableIssues();
                        foreach (var issue in result.Issues)
                        {
                            _statusAggregator.AddIssue(issue);
                        }
                    }
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[HealthService] 健康检查异常: {ex.Message}");
                }
            }
        }, _cts.Token);

        Console.WriteLine("[HealthService] 健康服务已启动");
    }

    public async Task StopAsync()
    {
        if (!_isRunning)
        {
            return;
        }

        _cts?.Cancel();

        if (_monitorTask != null)
        {
            try
            {
                await _monitorTask;
            }
            catch (OperationCanceledException)
            {
                // 正常取消
            }
        }

        _isRunning = false;
        Console.WriteLine("[HealthService] 健康服务已停止");
    }

    public void Dispose()
    {
        _cts?.Cancel();
        _resourceHealthChanged.OnCompleted();
        _resourceHealthChanged.Dispose();
        _cts?.Dispose();
    }
}
