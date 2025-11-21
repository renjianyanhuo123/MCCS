namespace MCCS.Infrastructure.TestModels.ControlParams
{
    public enum StaticLoadControlEnum : uint
    {
        /// <summary>
        /// 无
        /// </summary>
        CTRLMODE_None = 0,
        /// <summary>
        /// 力速度 + 力目标值
        /// </summary>
        CTRLMODE_LoadN = 1,
        /// <summary>
        /// 力保持
        /// </summary>
        CTRLMODE_HLoadN = 2,
        /// <summary>
        /// 位移速度 + 位移目标值
        /// </summary>
        CTRLMODE_LoadS = 3,
        /// <summary>
        /// 位移保持
        /// </summary>
        CTRLMODE_HLoadS = 4,
        /// <summary>
        /// 开环控制
        /// </summary>
        CTRLMODE_OPEN = 5,
        /// <summary>
        /// 不控
        /// </summary>
        CTRLMODE_NOCTRL = 6,
        /// <summary>
        /// 力跟踪
        /// </summary>
        CTRLMODE_TRACEN = 7,
        /// <summary>
        /// 位移跟踪
        /// </summary>
        CTRLMODE_TRACES = 8,
        /// <summary>
        /// 暂停控制
        /// </summary>
        CTRLMODE_HALTS = 9, 
        /// <summary>
        /// 力速度 + 位移目标值
        /// </summary>
        CTRLMODE_LoadNVSP = 11,
        /// <summary>
        /// 位移速度 + 力目标值
        /// </summary>
        CTRLMODE_LoadSVNP = 12,
    }

    public sealed record StaticControlParams
    { 
        /// <summary>
        /// 静态控制类型
        /// </summary>
        public StaticLoadControlEnum StaticLoadControl { get; init; }
        /// <summary>
        /// 速度
        /// kN(mm)/min
        /// </summary>
        public float Speed { get; init; }
        /// <summary>
        /// 目标值
        /// </summary>
        public float TargetValue { get; init; }
    }
}
