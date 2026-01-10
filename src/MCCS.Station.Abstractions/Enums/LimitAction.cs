namespace MCCS.Station.Abstractions.Enums;

/// <summary>
/// 限位触发后的动作
/// </summary>
public enum LimitAction : byte
{
    /// <summary>
    /// 仅警告，不采取动作
    /// </summary>
    WarnOnly = 0,

    /// <summary>
    /// 保持当前位置
    /// </summary>
    HoldPosition = 1,

    /// <summary>
    /// 保持当前力/载荷
    /// </summary>
    HoldLoad = 2,

    /// <summary>
    /// 限制速度
    /// </summary>
    LimitSpeed = 3,

    /// <summary>
    /// 禁止向该方向运动
    /// </summary>
    BlockDirection = 4,

    /// <summary>
    /// 冻结命令通道
    /// </summary>
    FreezeChannel = 5,

    /// <summary>
    /// 触发软停止
    /// </summary>
    SoftStop = 6,

    /// <summary>
    /// 触发联锁
    /// </summary>
    TriggerInterlock = 7,

    /// <summary>
    /// 触发紧急停止
    /// </summary>
    TriggerEStop = 8
}
