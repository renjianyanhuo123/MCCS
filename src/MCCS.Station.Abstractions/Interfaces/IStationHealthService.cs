using MCCS.Station.Abstractions.Enums;
using MCCS.Station.Abstractions.Events;
using MCCS.Station.Abstractions.Models;

namespace MCCS.Station.Abstractions.Interfaces;

/// <summary>
/// 站点健康服务接口
/// 负责周期性采样底层状态（来自采集进程/控制器）
/// 从资源树自底向上计算站点状态
/// </summary>
public interface IStationHealthService
{
    /// <summary>
    /// 资源健康变更事件流
    /// </summary>
    IObservable<ResourceHealthChangedEvent> ResourceHealthChanged { get; }

    /// <summary>
    /// 整体连接状态
    /// </summary>
    ConnectivityStatus OverallConnectivity { get; }

    /// <summary>
    /// 整体激活状态
    /// </summary>
    ActivationStatus OverallActivation { get; }

    /// <summary>
    /// 注册资源节点
    /// </summary>
    void RegisterResource(ResourceHealthInfo resource);

    /// <summary>
    /// 批量注册资源
    /// </summary>
    void RegisterResources(IEnumerable<ResourceHealthInfo> resources);

    /// <summary>
    /// 移除资源节点
    /// </summary>
    bool RemoveResource(string resourceId);

    /// <summary>
    /// 更新资源健康状态
    /// </summary>
    void UpdateResourceHealth(string resourceId, ResourceHealth health, FaultClass faultClass = FaultClass.None,
        string? details = null, string? faultCode = null);

    /// <summary>
    /// 获取所有资源健康信息
    /// </summary>
    IReadOnlyList<ResourceHealthInfo> GetAllResources();

    /// <summary>
    /// 获取指定类型的资源
    /// </summary>
    IReadOnlyList<ResourceHealthInfo> GetResourcesByType(ResourceType type);

    /// <summary>
    /// 获取指定资源的健康信息
    /// </summary>
    ResourceHealthInfo? GetResource(string resourceId);

    /// <summary>
    /// 获取资源树（按层级组织）
    /// </summary>
    IReadOnlyList<ResourceHealthInfo> GetResourceTree();

    /// <summary>
    /// 获取故障资源列表
    /// </summary>
    IReadOnlyList<ResourceHealthInfo> GetFaultedResources();

    /// <summary>
    /// 获取警告资源列表
    /// </summary>
    IReadOnlyList<ResourceHealthInfo> GetWarningResources();

    /// <summary>
    /// 计算聚合的连接状态
    /// </summary>
    ConnectivityStatus CalculateConnectivityStatus();

    /// <summary>
    /// 计算聚合的激活状态
    /// </summary>
    ActivationStatus CalculateActivationStatus();

    /// <summary>
    /// 计算聚合的能力
    /// </summary>
    CapabilityFlags CalculateCapabilities();

    /// <summary>
    /// 执行健康检查
    /// </summary>
    Task<HealthCheckResult> PerformHealthCheckAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// 启动健康监控
    /// </summary>
    Task StartAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// 停止健康监控
    /// </summary>
    Task StopAsync();

    /// <summary>
    /// 健康检查间隔（毫秒）
    /// </summary>
    int HealthCheckIntervalMs { get; set; }

    /// <summary>
    /// 是否正在运行
    /// </summary>
    bool IsRunning { get; }
}

/// <summary>
/// 健康检查结果
/// </summary>
public sealed class HealthCheckResult
{
    /// <summary>
    /// 是否健康
    /// </summary>
    public bool IsHealthy { get; init; }

    /// <summary>
    /// 连接状态
    /// </summary>
    public ConnectivityStatus Connectivity { get; init; }

    /// <summary>
    /// 激活状态
    /// </summary>
    public ActivationStatus Activation { get; init; }

    /// <summary>
    /// 可用能力
    /// </summary>
    public CapabilityFlags Capabilities { get; init; }

    /// <summary>
    /// 资源总数
    /// </summary>
    public int TotalResources { get; init; }

    /// <summary>
    /// 健康资源数
    /// </summary>
    public int HealthyResources { get; init; }

    /// <summary>
    /// 警告资源数
    /// </summary>
    public int WarningResources { get; init; }

    /// <summary>
    /// 故障资源数
    /// </summary>
    public int FaultedResources { get; init; }

    /// <summary>
    /// 问题列表
    /// </summary>
    public IReadOnlyList<StatusIssue> Issues { get; init; } = Array.Empty<StatusIssue>();

    /// <summary>
    /// 检查时间
    /// </summary>
    public DateTime CheckedAt { get; init; } = DateTime.UtcNow;

    /// <summary>
    /// 检查耗时（毫秒）
    /// </summary>
    public int DurationMs { get; init; }
}
