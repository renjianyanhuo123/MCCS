namespace MCCS.Station.Abstractions.Enums;

/// <summary>
/// 故障分类
/// 用于指示故障的严重程度和处理方式
/// </summary>
public enum FaultClass : byte
{
    /// <summary>
    /// 无故障
    /// </summary>
    None = 0,

    /// <summary>
    /// 可恢复故障
    /// 系统可自动或操作员简单操作恢复
    /// </summary>
    Recoverable = 1,

    /// <summary>
    /// 联锁级故障
    /// 需要清除联锁/恢复现场才能继续
    /// </summary>
    Interlock = 2,

    /// <summary>
    /// 失控保护级故障
    /// 必须人工干预和排障
    /// </summary>
    Failsafe = 3
}
