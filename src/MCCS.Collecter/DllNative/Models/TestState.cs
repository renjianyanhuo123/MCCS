namespace MCCS.Collecter.DllNative.Models
{
    /// <summary>
    /// 试验状态
    /// </summary>
    public enum TestState : byte
    {
        /// <summary>停止</summary>
        Stop = 0,
        /// <summary>运行</summary>
        Running = 1,
        /// <summary>暂停</summary>
        Pause = 2
    }
}
