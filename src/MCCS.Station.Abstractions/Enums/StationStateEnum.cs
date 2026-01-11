namespace MCCS.Station.Abstractions.Enums;

/// <summary>
/// 站点状态
/// </summary>
public enum StationStateEnum : byte
{
    Offline,        // 离线，未连接控制器
    Connecting,     // 正在连接
    Online,         // 已连接但未就绪
    Ready,          // 设备就绪，可以开始试验
    Running,        // 正在执行段
    Paused,         // 暂停
    Faulted,        // 故障状态
    EStop,          // 急停状态
    Recovering      // 恢复中
}