namespace MCCS.Station.Abstractions.Enums;

/// <summary>
/// 激活与能量维度状态
/// 回答：能量链路是否打开？（液压/伺服阀台/驱动电源）
/// </summary>
public enum ActivationStatus : byte
{
    /// <summary>
    /// 能量关闭
    /// </summary>
    Off = 0,

    /// <summary>
    /// 正在激活（过渡态）
    /// </summary>
    Activating = 1,

    /// <summary>
    /// 低压/待机模式（阀台开启但未高压）
    /// </summary>
    Low = 2,

    /// <summary>
    /// 正在升压（过渡态）
    /// </summary>
    Pressurizing = 3,

    /// <summary>
    /// 高压/完全激活（可进行控制）
    /// </summary>
    High = 4,

    /// <summary>
    /// 正在卸压（过渡态）
    /// </summary>
    Depressurizing = 5,

    /// <summary>
    /// 正在停止（过渡态）
    /// </summary>
    Deactivating = 6
}
