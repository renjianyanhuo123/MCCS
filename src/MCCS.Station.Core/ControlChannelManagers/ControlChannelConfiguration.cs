using MCCS.Station.Core.DllNative.Models;
using MCCS.Station.Core.HardwareDevices;

namespace MCCS.Station.Core.ControlChannelManagers
{
    public enum ControlChannelSignalTypeEnum : int
    {
        /// <summary>
        /// 位置反馈
        /// </summary>
        Position,
        /// <summary>
        /// 力反馈
        /// </summary>
        Force,
        /// <summary>
        /// 输出信号
        /// </summary>
        Output
    }

    public record ControlChannelConfiguration
    {
        // public required ControlCompletionConfiguration CancellationConfiguration { get; init; }
        public long ChannelId { get; init; }

        public string ChannelName { get; init; } = string.Empty;
        /// <summary>
        /// 每一个控制通道都必须要一个主控制器
        /// </summary>
        public long ControllerId { get; init; }

        public required List<ControlChannelSignalConfiguration> SignalConfiguration { get; init; }
    }

    public record ControlChannelSignalConfiguration
    {
        public long SignalId { get; set; } 
        public ControlChannelSignalTypeEnum SignalType { get; set; }  // 信号类型 
    }
}
