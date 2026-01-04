namespace MCCS.Station.DllNative.Models
{
    /// <summary>
    /// 波形类型
    /// </summary>
    public enum WaveShape : byte
    {
        /// <summary>正弦波</summary>
        Sine = 0,
        /// <summary>三角波</summary>
        Triangle = 1,
        /// <summary>方波</summary>
        Square = 2,
        /// <summary>文件波</summary>
        File = 3
    }
}
