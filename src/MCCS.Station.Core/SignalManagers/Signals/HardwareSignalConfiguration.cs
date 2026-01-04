using MCCS.Station.Core.DllNative.Models;
using MCCS.Station.Core.HardwareDevices;

namespace MCCS.Station.Core.SignalManagers.Signals
{
    /// <summary>
    /// Bw控制器所有的信号接口信息
    /// 物理接口
    /// </summary>
    public record HardwareSignalConfiguration
    {
        public long SignalId { get; set; }                            // 信号物理ID，如 "AI0", "AO1"
        /// <summary>
        /// 所属的控制器ID
        /// </summary>
        public long BelongControllerId { get; set; }
        public string SignalName { get; set; } = string.Empty;        // 信号名称
        public SignalAddressEnum SignalAddress { get; init; }         // 信号地址
        public SignalType SignalType { get; set; }                   // 信号类型
        public SignalDirection Direction { get; set; }                // 信号方向
        public double MinValue { get; set; }                           // 量程下限
        public double MaxValue { get; set; }                           // 量程上限
        public string Unit { get; set; } = string.Empty;               // 工程单位
        public string Description { get; set; } = string.Empty;        // 信号描述
        public bool IsEnabled { get; set; } = true;                    // 是否启用
        /// <summary>
        /// 所连接的设备ID
        /// </summary>
        public long? DeviceId { get; set; } = 0;
        public int SampleRate { get; set; } = 50;
        public int BufferSize { get; set; } = 1000;                     // 每个信号独立的缓冲区大小
    }
}
