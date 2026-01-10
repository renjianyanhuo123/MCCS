namespace MCCS.Station.Abstractions.Enums;

/// <summary>
/// 能力标志
/// 指示当前站点允许的操作类型
/// </summary>
[Flags]
public enum CapabilityFlags : uint
{
    /// <summary>
    /// 无能力
    /// </summary>
    None = 0,

    /// <summary>
    /// 可连接/断开
    /// </summary>
    CanConnect = 1 << 0,

    /// <summary>
    /// 可激活（开启阀台/供能）
    /// </summary>
    CanActivate = 1 << 1,

    /// <summary>
    /// 可升高压
    /// </summary>
    CanPressurize = 1 << 2,

    /// <summary>
    /// 可运动控制
    /// </summary>
    CanMove = 1 << 3,

    /// <summary>
    /// 可闭环控制
    /// </summary>
    CanControl = 1 << 4,

    /// <summary>
    /// 可启动试验
    /// </summary>
    CanStartTest = 1 << 5,

    /// <summary>
    /// 可暂停试验
    /// </summary>
    CanPause = 1 << 6,

    /// <summary>
    /// 可恢复试验
    /// </summary>
    CanResume = 1 << 7,

    /// <summary>
    /// 可停止试验
    /// </summary>
    CanStop = 1 << 8,

    /// <summary>
    /// 可手动控制
    /// </summary>
    CanManualControl = 1 << 9,

    /// <summary>
    /// 可数据记录
    /// </summary>
    CanRecord = 1 << 10,

    /// <summary>
    /// 可清除联锁
    /// </summary>
    CanClearInterlock = 1 << 11,

    /// <summary>
    /// 可复位急停
    /// </summary>
    CanResetEStop = 1 << 12,

    /// <summary>
    /// 可修改参数
    /// </summary>
    CanModifyParameters = 1 << 13,

    /// <summary>
    /// 可零点标定（Tare）
    /// </summary>
    CanTare = 1 << 14,

    /// <summary>
    /// 可校准
    /// </summary>
    CanCalibrate = 1 << 15,

    /// <summary>
    /// 只读监视模式
    /// </summary>
    ReadOnly = 1 << 30,

    /// <summary>
    /// 完全能力（除ReadOnly外的所有能力）
    /// </summary>
    Full = CanConnect | CanActivate | CanPressurize | CanMove | CanControl |
           CanStartTest | CanPause | CanResume | CanStop | CanManualControl |
           CanRecord | CanClearInterlock | CanResetEStop | CanModifyParameters |
           CanTare | CanCalibrate
}
