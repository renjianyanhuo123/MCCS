namespace MCCS.Infrastructure.TestModels.Commands
{
    public enum CommandTypeEnum:int
    {
        // 通用指令
        Reset,
        Calibrate,
        SetParameter,
        GetParameter,
        StartMeasurement,
        Stop,

        // 特定设备指令
        SetMove,
        SetPressureUnit,
        SetSamplingRate,
        TriggerSelfTest,

        // 控制指令
        OpenValve,
        CloseValve,
        SetOutput,
        Emergency
    }
}
