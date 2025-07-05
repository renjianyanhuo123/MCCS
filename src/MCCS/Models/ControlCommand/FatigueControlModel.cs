namespace MCCS.Models.ControlCommand
{
    public class FatigueControlModel
    {
        /// <summary>
        /// 控制单位
        /// </summary>
        public ControlUnitTypeEnum ControlUnitType { get; set; }

        /// <summary>
        /// 波形类型
        /// </summary>
        public WaveformTypeEnum WaveformType { get; set; }
        /// <summary>
        /// 频率
        /// </summary>
        public double Frequency { get; set; }
        /// <summary>
        /// 幅值
        /// </summary>
        public double Amplitude { get; set; }
        /// <summary>
        /// 中值
        /// </summary>
        public double Median { get; set; }
        /// <summary>
        /// 循环次数
        /// </summary>
        public int CycleTimes { get; set; }
        /// <summary>
        /// 循环计数
        /// </summary>
        public int CycleCount { get; set; }
    }
}
