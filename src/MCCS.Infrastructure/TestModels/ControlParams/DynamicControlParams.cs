namespace MCCS.Infrastructure.TestModels.ControlParams
{
    public sealed record DynamicControlParams
    {
        /// <summary>
        /// 设备ID
        /// </summary>
        public long DeviceId { get; init; }
        /// <summary>
        /// 信号接口ID;
        /// 用于直接控制;
        /// </summary>
        public long SignalId { get; init; }
        /// <summary>
        /// 控制模式：0=位移，1=力
        /// </summary>
        public int ControlMode { get; init; }
        /// <summary>
        /// 波形类型：0=正弦，1=三角，2=方波
        /// </summary>
        public int WaveType { get; init; }
        /// <summary>
        /// 频率 Hz
        /// </summary>
        public float Frequency { get; init; }
        /// <summary>
        /// 幅值 mm 或 kN
        /// </summary>
        public float Amplitude { get; init; }
        /// <summary>
        /// 中值 mm 或 kN
        /// </summary>
        public float MeanValue { get; init; }
        /// <summary>
        /// 补偿幅值
        /// </summary>
        public float CompensateAmplitude { get; init; }
        /// <summary>
        /// 补偿相位
        /// </summary>
        public float CompensationPhase { get; init; }
        /// <summary>
        /// 是否调整中值
        /// </summary>
        public bool IsAdjustedMedian { get; init; }

        /// <summary>
        /// 循环次数
        /// </summary>
        public int CycleCount { get; init; }
    }
}
