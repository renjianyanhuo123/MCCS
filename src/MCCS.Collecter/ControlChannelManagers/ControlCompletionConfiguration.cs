namespace MCCS.Station.ControlChannelManagers 
{
    public sealed record ControlCompletionConfiguration
    {
        /// <summary>
        /// 位移允许误差 (mm)
        /// </summary>
        public double DisplacementTolerance { get; set; } = 0.01;

        /// <summary>
        /// 力允许误差 (kN) - 可以设为满量程的百分比
        /// </summary>
        public double ForceTolerance { get; set; } = 0.02;

        /// <summary>
        /// 位置误差阈值 (控制器返回的PosE)
        /// </summary>
        public double PosErrorThreshold { get; set; } = 0.01;

        /// <summary>
        /// 控制输出稳定阈值
        /// </summary>
        public double ControlOutputThreshold { get; set; } = 0.05;

        /// <summary>
        /// 超时时间 (毫秒)
        /// </summary>
        public int TimeoutMs { get; set; } = 6000;

        /// <summary>
        /// 需要连续稳定的采样次数
        /// </summary>
        public int RequiredStableCount { get; set; } = 10;

        /// <summary>
        /// 是否启用自适应阈值（根据传感器量程自动计算）
        /// </summary>
        public bool EnableAdaptiveTolerance { get; set; } = false;

        /// <summary>
        /// 自适应阈值百分比（当启用自适应时）
        /// </summary>
        public double AdaptiveTolerancePercentage { get; set; } = 0.005; // 0.5%
    }
}
