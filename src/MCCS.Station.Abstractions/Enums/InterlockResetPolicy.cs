namespace MCCS.Station.Abstractions.Enums;

/// <summary>
/// 联锁复位策略
/// 指示如何复位/清除联锁
/// </summary>
public enum InterlockResetPolicy : byte
{
    /// <summary>
    /// 自动复位：条件恢复后自动清除
    /// </summary>
    Auto = 0,

    /// <summary>
    /// 手动复位：条件恢复后需操作员确认清除
    /// </summary>
    Manual = 1,

    /// <summary>
    /// 带确认的手动复位：需要操作员确认并输入原因
    /// </summary>
    ManualWithConfirm = 2,

    /// <summary>
    /// 硬件复位：需要物理操作（如复位急停按钮）
    /// </summary>
    Hardware = 3,

    /// <summary>
    /// 维护复位：需要维护人员介入
    /// </summary>
    Maintenance = 4
}
