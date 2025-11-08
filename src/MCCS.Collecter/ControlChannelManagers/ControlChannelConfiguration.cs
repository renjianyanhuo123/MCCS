using MCCS.Collecter.DllNative.Models;
using MCCS.Collecter.HardwareDevices;

namespace MCCS.Collecter.ControlChannelManagers
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
        public ControlCompletionConfiguration CancellationConfiguration { get; init; }

        public long ChannelId { get; init; }

        public string ChannelName { get; init; } = string.Empty;

        public List<ControlChannelSignalConfiguration> SignalConfiguration { get; init; }
    }

    public record ControlChannelSignalConfiguration
    {
        public long SignalId { get; set; }                            // 信号物理ID，如 "AI0", "AO1"
        /// <summary>
        /// 所属的控制器ID
        /// </summary>
        public long BelongControllerId { get; set; }
        public string SignalName { get; set; } = string.Empty;        // 信号名称
        public SignalAddressEnum SignalAddress { get; init; }         // 信号地址
        public ControlChannelSignalTypeEnum SignalType { get; set; }  // 信号类型
        public SignalDirection Direction { get; set; }                // 信号方向
        public double MinValue { get; set; }                           // 量程下限
        public double MaxValue { get; set; }                           // 量程上限
        public string Unit { get; set; } = string.Empty;               // 工程单位
        public string Description { get; set; } = string.Empty;        // 信号描述 
        /// <summary>
        /// 所连接的设备ID
        /// </summary>
        public long? DeviceId { get; set; } = 0;
        public int SampleRate { get; set; } = 50;
        public int BufferSize { get; set; } = 1000;                     // 每个信号独立的缓冲区大小
    }
}
