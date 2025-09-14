namespace MCCS.Collecter.HardwareDevices
{
    public enum SignalType
    {
        AnalogInput,    // 模拟量输入 (AI)
        AnalogOutput,   // 模拟量输出 (AO) 
        DigitalInput,   // 数字量输入 (DI)
        DigitalOutput,  // 数字量输出 (DO)
        Counter,        // 计数器
        PWM            // 脉宽调制输出
    }
}
