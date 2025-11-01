namespace MCCS.Infrastructure.TestModels.ControlParams
{
    public enum StaticLoadControlEnum : uint
    {
        /// <summary>
        /// 力速度 + 力目标值
        /// </summary>
        CTRLMODE_LoadN = 1,
        /// <summary>
        /// 位移速度 + 位移目标值
        /// </summary>
        CTRLMODE_LoadS = 2,
        /// <summary>
        /// 位移速度 + 力目标值
        /// </summary>
        CTRLMODE_LoadSVNP = 3,
        /// <summary>
        /// 力速度 + 位移目标值
        /// </summary>
        CTRLMODE_LoadNVSP = 4   
    }

    public sealed record StaticControlParams
    {
        /// <summary>
        /// 静态控制类型
        /// </summary>
        public StaticLoadControlEnum StaticLoadControl { get; init; }
        /// <summary>
        /// 速度
        /// </summary>
        public float Speed { get; init; }
        /// <summary>
        /// 目标值
        /// </summary>
        public float TargetValue { get; init; }
    }
}
