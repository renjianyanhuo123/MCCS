namespace MCCS.Station.Abstractions.Enums;

/// <summary>
/// 安全与保护维度状态
/// 回答：是否触发保护/联锁/限位？是否允许继续？
/// 安全系统独立于状态机，对状态机有"硬打断/降级/锁定"的权限
/// </summary>
public enum SafetyStatus : byte
{
    /// <summary>
    /// 正常运行，无安全警告
    /// </summary>
    Normal = 0,

    /// <summary>
    /// 警告状态：接近限位、趋势异常但未触发保护
    /// 允许继续但应注意
    /// </summary>
    Warning = 1,

    /// <summary>
    /// 受限状态：触发过程保护/软限位
    /// 保持或限制动作，允许部分操作但强约束
    /// </summary>
    Limited = 2,

    /// <summary>
    /// 联锁触发状态
    /// 需要"恢复现场/清除联锁"才能继续
    /// </summary>
    Interlocked = 3,

    /// <summary>
    /// 急停/失控保护状态
    /// 已切断能量链路，必须人工干预排障与复位
    /// </summary>
    Failsafe = 4,

    /// <summary>
    /// 急停按下状态（硬件级急停）
    /// 必须物理复位急停按钮后才能恢复
    /// </summary>
    EStop = 5
}
