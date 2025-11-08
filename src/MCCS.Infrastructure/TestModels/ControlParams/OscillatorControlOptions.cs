namespace MCCS.Infrastructure.TestModels.ControlParams
{
    [Flags]
    public enum OscillatorControlOptions
    {
        None = 1 << 0,           // 1   无
        MeanAmplitude = 1 << 1,  // 2   启用中值控制
        Start = 1 << 2,          // 4   启用起振过程
        Stop = 1 << 3,           // 8   启用停振过程
        All = MeanAmplitude | Start | Stop
    }
}
