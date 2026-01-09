using System.Runtime.InteropServices;

namespace MCCS.Station.Abstractions.Communication;

/// <summary>
/// 通道数据包 - 用于共享内存传输的数据结构
/// 固定大小结构，适合高性能传输
/// </summary>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct ChannelDataPacket
{
    /// <summary>
    /// 消息类型
    /// </summary>
    public MessageType Type;

    /// <summary>
    /// 通道ID（信号ID或虚拟通道ID）
    /// </summary>
    public long ChannelId;

    /// <summary>
    /// 时间戳（Ticks）
    /// </summary>
    public long Timestamp;

    /// <summary>
    /// 数据值
    /// </summary>
    public double Value;

    /// <summary>
    /// 数据质量
    /// </summary>
    public DataQuality Quality;

    /// <summary>
    /// 序列号（用于检测数据丢失）
    /// </summary>
    public long SequenceNumber;

    /// <summary>
    /// 保留字段（用于对齐和未来扩展）
    /// </summary>
    public long Reserved;
}

/// <summary>
/// 批量通道数据包头
/// </summary>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct BatchDataHeader
{
    /// <summary>
    /// 消息类型
    /// </summary>
    public MessageType Type;

    /// <summary>
    /// 批次ID
    /// </summary>
    public long BatchId;

    /// <summary>
    /// 时间戳
    /// </summary>
    public long Timestamp;

    /// <summary>
    /// 数据项数量
    /// </summary>
    public int ItemCount;

    /// <summary>
    /// 序列号
    /// </summary>
    public long SequenceNumber;
}

/// <summary>
/// 批量数据中的单个数据项
/// </summary>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct BatchDataItem
{
    /// <summary>
    /// 通道ID
    /// </summary>
    public long ChannelId;

    /// <summary>
    /// 数据值
    /// </summary>
    public double Value;

    /// <summary>
    /// 数据质量
    /// </summary>
    public DataQuality Quality;
}
