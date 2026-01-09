namespace MCCS.Station.Abstractions.Communication;

/// <summary>
/// 共享内存通信消息类型
/// </summary>
public enum MessageType : byte
{
    /// <summary>
    /// 无效消息
    /// </summary>
    None = 0,

    /// <summary>
    /// 心跳消息 - 用于检测连接状态
    /// </summary>
    Heartbeat = 1,

    /// <summary>
    /// 单个信号数据点
    /// </summary>
    SignalData = 10,

    /// <summary>
    /// 批量信号数据
    /// </summary>
    SignalDataBatch = 11,

    /// <summary>
    /// 虚拟通道数据点
    /// </summary>
    PseudoChannelData = 20,

    /// <summary>
    /// 批量虚拟通道数据
    /// </summary>
    PseudoChannelDataBatch = 21,

    /// <summary>
    /// 曲线数据点（用于实时曲线显示）
    /// </summary>
    CurveData = 30,

    /// <summary>
    /// 系统状态信息
    /// </summary>
    SystemStatus = 40,

    /// <summary>
    /// 控制器状态
    /// </summary>
    ControllerStatus = 41,

    /// <summary>
    /// 命令请求
    /// </summary>
    CommandRequest = 50,

    /// <summary>
    /// 命令响应
    /// </summary>
    CommandResponse = 51,

    /// <summary>
    /// 错误消息
    /// </summary>
    Error = 255
}
