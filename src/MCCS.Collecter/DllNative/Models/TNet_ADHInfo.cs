using System.Runtime.InteropServices;

namespace MCCS.Station.DllNative.Models
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct TNet_ADHInfo()
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 6)]
        public float[] Net_AD_N = new float[6];              // 6路AI通道采样值

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
        public float[] Net_AD_S = new float[2];              // 2路SSI通道采样值

        public float Net_PosVref;             // 位置给定值
        public float Net_PosE;                // 位置误差
        public float Net_CtrlDA;              // 控制输出DA值
        public int Net_CycleCount;            // 循环计数
        public int Net_SysState;              // 系统状态
        public int Net_DIVal;                 // 数字输入值
        public int Net_DOVal;                 // 数字输出值
        public float Net_D_PosVref;           // 动态调整后的信号值
        public float Net_FeedLoadN;           // 试验力反馈值(关键!)
        public int Net_PrtErrState;           // 保护错误代码
        public int Net_TimeCnt;               // 时间计数器

        // 构造函数用于初始化数组
    }
}
