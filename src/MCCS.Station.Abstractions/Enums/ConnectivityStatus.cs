namespace MCCS.Station.Abstractions.Enums;

/// <summary>
/// 连接与资源维度状态
/// 回答：物理资源是否齐全、是否可用？
/// </summary>
public enum ConnectivityStatus : byte
{
    /// <summary>
    /// 控制器/子进程/网络未连上
    /// </summary>
    Disconnected = 0,

    /// <summary>
    /// 正在建立连接
    /// </summary>
    Connecting = 1,

    /// <summary>
    /// 连上但缺硬件/通道映射不完整/部分资源失效
    /// </summary>
    Degraded = 2,

    /// <summary>
    /// 硬件资源完整、配置一致、关键自检通过
    /// </summary>
    Ready = 3
}
