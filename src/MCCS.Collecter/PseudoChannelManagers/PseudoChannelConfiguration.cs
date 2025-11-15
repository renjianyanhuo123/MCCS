using MCCS.Collecter.SignalManagers.Signals;

namespace MCCS.Collecter.PseudoChannelManagers
{
    public class PseudoChannelConfiguration
    {
        public required long ChannelId { get; set; }

        public required string ChannelName { get; set; }
        /// <summary>
        /// 范围最小值
        /// </summary>
        public double RangeMin { get; set; }
        /// <summary>
        /// 范围最大值
        /// </summary>
        public double RangeMax { get; set; }
        /// <summary>
        /// 计算公式
        /// </summary>
        public string Formula { get; set; } = string.Empty;
        /// <summary>
        /// 单位;None---默认无单位
        /// </summary>
        public string? Unit { get; set; } = null;

        /// <summary>
        /// 是否可校准
        /// </summary>
        public bool HasTare { get; set; }
        /// <summary>
        /// 所有的信号集合配置
        /// </summary>
        public List<HardwareSignalConfiguration> SignalConfigurations { get; set; }
    }
}
