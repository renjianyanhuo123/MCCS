namespace MCCS.Station.Abstractions.Enums;

/// <summary>
/// 资源健康状态
/// 用于描述单个资源节点（物理/逻辑）的健康程度
/// </summary>
public enum ResourceHealth : byte
{
    /// <summary>
    /// 未知状态（尚未检测）
    /// </summary>
    Unknown = 0,

    /// <summary>
    /// 正常健康
    /// </summary>
    Ok = 1,

    /// <summary>
    /// 警告（存在隐患但可工作）
    /// </summary>
    Warning = 2,

    /// <summary>
    /// 故障
    /// </summary>
    Fault = 3
}
