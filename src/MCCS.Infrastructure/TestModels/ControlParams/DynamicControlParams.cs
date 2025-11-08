namespace MCCS.Infrastructure.TestModels.ControlParams
{
    public sealed record DynamicControlParams
    { 
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
        /// 是否启用中值控制
        /// 启用起振过程
        /// 启用停振过程
        /// </summary>
        private OscillatorControlOptions _options = OscillatorControlOptions.All;
        public OscillatorControlOptions CtrlOpt {
            get
            {
                if (Frequency < 1)
                {
                    _options &= ~OscillatorControlOptions.Start;
                    _options &= ~OscillatorControlOptions.Stop;
                }
                return _options;
            }
        }

        /// <summary>
        /// 超限次数
        /// </summary> 
        public int ValleyPeakFilterNum => Frequency < 1 ? 1 : 5;

        /// <summary>
        /// 循环次数
        /// </summary>
        public int CycleCount { get; init; }
    }
}
