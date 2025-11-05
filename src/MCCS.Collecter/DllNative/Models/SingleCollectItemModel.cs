namespace MCCS.Collecter.DllNative.Models
{
    public record SingleCollectItemModel
    { 
        /// <summary>
        /// 所有采集信号的值
        /// </summary>
        public Dictionary<long, double> Values { get; init; } = new();

        /// <summary>
        /// 位置给定值
        /// </summary>
        public float Net_PosVref { get; init; }

        /// <summary>
        /// 位置误差
        /// </summary>
        public float Net_PosE { get; init; }

        /// <summary>
        /// 控制输出DA值
        /// </summary>
        public float Net_CtrlDA { get; init; }

        /// <summary>
        /// 循环计数
        /// </summary>
        public int Net_CycleCount { get; init; }

        /// <summary>
        /// 系统状态
        /// </summary>
        public int Net_SysState { get; init; }

        /// <summary>
        /// 数字输入值
        /// </summary>
        public int Net_DIVal { get; init; }

        /// <summary>
        /// 数字输出值
        /// </summary> 
        public int Net_DOVal { get; init; }

        /// <summary>
        /// 动态调整后的信号值
        /// </summary> 
        public float Net_D_PosVref { get; init; }

        /// <summary>
        /// 试验力反馈值(关键!)
        /// </summary> 
        public float Net_FeedLoadN { get; init; }
        /// <summary>
        /// 保护错误代码
        /// </summary>
        public int Net_PrtErrState { get; init; }
        /// <summary>
        /// 时间计数器
        /// </summary>
        public int Net_TimeCnt { get; init; }
    }
}
