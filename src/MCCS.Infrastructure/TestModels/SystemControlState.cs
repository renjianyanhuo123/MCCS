namespace MCCS.Infrastructure.TestModels
{
    /// <summary>
    /// 系统控制状态
    /// </summary>
    public enum SystemControlState : byte
    {
        /// <summary>开环控制</summary>
        OpenLoop = 0,
        /// <summary>静态控制</summary>
        Static = 1,
        /// <summary>动态控制</summary>
        Dynamic = 2
    }
}
