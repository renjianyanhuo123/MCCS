using MCCS.Station.Abstractions.Enums;
using MCCS.Station.Abstractions.Models;

namespace MCCS.Station.Abstractions.Events;

/// <summary>
/// 资源健康状态变更事件
/// </summary>
public record ResourceHealthChangedEvent : StationEvent
{
    /// <summary>
    /// 资源ID
    /// </summary>
    public string ResourceId { get; init; } = string.Empty;

    /// <summary>
    /// 资源名称
    /// </summary>
    public string ResourceName { get; init; } = string.Empty;

    /// <summary>
    /// 资源类型
    /// </summary>
    public ResourceType ResourceType { get; init; }

    /// <summary>
    /// 之前的健康状态
    /// </summary>
    public ResourceHealth PreviousHealth { get; init; }

    /// <summary>
    /// 当前健康状态
    /// </summary>
    public ResourceHealth CurrentHealth { get; init; }

    /// <summary>
    /// 故障分类
    /// </summary>
    public FaultClass FaultClass { get; init; }

    /// <summary>
    /// 故障代码
    /// </summary>
    public string? FaultCode { get; init; }

    /// <summary>
    /// 详细信息
    /// </summary>
    public string Details { get; init; } = string.Empty;

    /// <summary>
    /// 诊断数据
    /// </summary>
    public IReadOnlyDictionary<string, object>? Diagnostics { get; init; }
}
