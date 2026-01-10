using MCCS.Station.Abstractions.Enums;

namespace MCCS.Station.Abstractions.Models;

/// <summary>
/// 资源健康信息
/// 每个资源节点（物理/逻辑）的统一健康输出
/// </summary>
public sealed class ResourceHealthInfo
{
    /// <summary>
    /// 资源唯一标识
    /// </summary>
    public string ResourceId { get; init; } = string.Empty;

    /// <summary>
    /// 资源名称
    /// </summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// 资源类型
    /// </summary>
    public ResourceType Type { get; init; }

    /// <summary>
    /// 父资源ID（用于构建资源树）
    /// </summary>
    public string? ParentId { get; init; }

    /// <summary>
    /// 健康状态
    /// </summary>
    public ResourceHealth Health { get; init; } = ResourceHealth.Unknown;

    /// <summary>
    /// 故障分类
    /// </summary>
    public FaultClass FaultClass { get; init; } = FaultClass.None;

    /// <summary>
    /// 该资源当前的能力
    /// </summary>
    public CapabilityFlags Capabilities { get; init; } = CapabilityFlags.None;

    /// <summary>
    /// 状态详情
    /// </summary>
    public string Details { get; init; } = string.Empty;

    /// <summary>
    /// 故障代码（如果有故障）
    /// </summary>
    public string? FaultCode { get; init; }

    /// <summary>
    /// 最后更新时间
    /// </summary>
    public DateTime LastUpdated { get; init; } = DateTime.UtcNow;

    /// <summary>
    /// 最后在线时间
    /// </summary>
    public DateTime? LastOnline { get; init; }

    /// <summary>
    /// 子资源健康汇总（可选，用于聚合）
    /// </summary>
    public IReadOnlyList<ResourceHealthInfo>? Children { get; init; }

    /// <summary>
    /// 诊断数据（可选）
    /// </summary>
    public IReadOnlyDictionary<string, object>? Diagnostics { get; init; }

    /// <summary>
    /// 是否健康
    /// </summary>
    public bool IsHealthy => Health == ResourceHealth.Ok;

    /// <summary>
    /// 是否有故障
    /// </summary>
    public bool HasFault => Health == ResourceHealth.Fault;

    /// <summary>
    /// 是否需要联锁
    /// </summary>
    public bool RequiresInterlock => FaultClass >= FaultClass.Interlock;
}

/// <summary>
/// 资源类型
/// </summary>
public enum ResourceType : byte
{
    /// <summary>
    /// 控制器
    /// </summary>
    Controller,

    /// <summary>
    /// IO模块
    /// </summary>
    IoModule,

    /// <summary>
    /// 传感器
    /// </summary>
    Sensor,

    /// <summary>
    /// 驱动器/伺服
    /// </summary>
    Drive,

    /// <summary>
    /// 阀门/阀台
    /// </summary>
    Valve,

    /// <summary>
    /// 油源
    /// </summary>
    HydraulicPowerUnit,

    /// <summary>
    /// 控制通道
    /// </summary>
    ControlChannel,

    /// <summary>
    /// 虚拟通道
    /// </summary>
    PseudoChannel,

    /// <summary>
    /// 信号
    /// </summary>
    Signal,

    /// <summary>
    /// 同步组
    /// </summary>
    SyncGroup,

    /// <summary>
    /// 试验模板/流程
    /// </summary>
    TestProfile,

    /// <summary>
    /// 通信链路
    /// </summary>
    Communication,

    /// <summary>
    /// 其他
    /// </summary>
    Other
}
