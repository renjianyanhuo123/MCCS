using System.Runtime.InteropServices;

namespace MCCS.Infrastructure.Communication;

/// <summary>
/// 通道数据项结构体
/// 用于共享内存传输的数据格式
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public struct ChannelDataItem
{
    /// <summary>
    /// 通道ID（虚拟通道ID）
    /// </summary>
    public long ChannelId;

    /// <summary>
    /// 当前批次索引(用于时间对齐)
    /// </summary>
    public long SequenceIndex;

    /// <summary>
    /// 采集值
    /// </summary>
    public double Value;
}
