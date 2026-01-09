using System.Runtime.InteropServices;

namespace MCCS.Station.Abstractions.Communication;

/// <summary>
/// 系统运行状态
/// </summary>
public enum SystemState : byte
{
    /// <summary>
    /// 未知状态
    /// </summary>
    Unknown = 0,

    /// <summary>
    /// 初始化中
    /// </summary>
    Initializing = 1,

    /// <summary>
    /// 就绪（空闲）
    /// </summary>
    Ready = 2,

    /// <summary>
    /// 运行中（采集数据）
    /// </summary>
    Running = 3,

    /// <summary>
    /// 暂停
    /// </summary>
    Paused = 4,

    /// <summary>
    /// 停止
    /// </summary>
    Stopped = 5,

    /// <summary>
    /// 错误状态
    /// </summary>
    Error = 255
}

/// <summary>
/// 系统状态数据包
/// </summary>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct SystemStatusPacket
{
    /// <summary>
    /// 消息类型
    /// </summary>
    public MessageType Type;

    /// <summary>
    /// 时间戳
    /// </summary>
    public long Timestamp;

    /// <summary>
    /// 系统状态
    /// </summary>
    public SystemState State;

    /// <summary>
    /// 活跃控制器数量
    /// </summary>
    public int ActiveControllerCount;

    /// <summary>
    /// 活跃信号数量
    /// </summary>
    public int ActiveSignalCount;

    /// <summary>
    /// 数据采样率（Hz）
    /// </summary>
    public int SampleRate;

    /// <summary>
    /// 心跳计数
    /// </summary>
    public long HeartbeatCount;

    /// <summary>
    /// 总发送数据包数
    /// </summary>
    public long TotalPacketsSent;

    /// <summary>
    /// 错误码（0表示无错误）
    /// </summary>
    public int ErrorCode;
}

/// <summary>
/// 控制器状态数据包
/// </summary>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct ControllerStatusPacket
{
    /// <summary>
    /// 消息类型
    /// </summary>
    public MessageType Type;

    /// <summary>
    /// 控制器ID
    /// </summary>
    public long ControllerId;

    /// <summary>
    /// 时间戳
    /// </summary>
    public long Timestamp;

    /// <summary>
    /// 是否连接
    /// </summary>
    public byte IsConnected;

    /// <summary>
    /// 是否正在采集
    /// </summary>
    public byte IsAcquiring;

    /// <summary>
    /// 采样率
    /// </summary>
    public int SampleRate;

    /// <summary>
    /// 已采集样本数
    /// </summary>
    public long SamplesCollected;

    /// <summary>
    /// 错误码
    /// </summary>
    public int ErrorCode;
}
