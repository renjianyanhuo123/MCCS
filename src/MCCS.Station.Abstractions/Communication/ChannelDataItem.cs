using System.Runtime.InteropServices;

namespace MCCS.Station.Abstractions.Communication
{
    [StructLayout(LayoutKind.Sequential)]
    public struct ChannelDataItem
    {
        /// <summary>
        /// 通道ID（虚拟通道ID）
        /// </summary>
        public long ChannelId;
        /// <summary>
        /// 采集值
        /// </summary>
        public double Value;
    }
}
