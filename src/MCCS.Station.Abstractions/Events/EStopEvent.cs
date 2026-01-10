using MCCS.Station.Abstractions.Enums;

namespace MCCS.Station.Abstractions.Events;

/// <summary>
/// 急停事件
/// 急停按下或释放时发出
/// </summary>
public record EStopEvent : StationEvent
{
    /// <summary>
    /// 急停类型
    /// </summary>
    public EStopType Type { get; init; }

    /// <summary>
    /// 是否激活（true=急停按下，false=急停释放）
    /// </summary>
    public bool IsActivated { get; init; }

    /// <summary>
    /// 触发源（哪个急停按钮/软件急停）
    /// </summary>
    public string TriggerSource { get; init; } = string.Empty;

    /// <summary>
    /// 执行的动作
    /// </summary>
    public IReadOnlyList<string> ActionsTaken { get; init; } = Array.Empty<string>();

    /// <summary>
    /// 是否需要硬件复位
    /// </summary>
    public bool RequiresHardwareReset { get; init; }

    /// <summary>
    /// 复位指令
    /// </summary>
    public string ResetInstructions { get; init; } = string.Empty;
}

/// <summary>
/// 急停类型
/// </summary>
public enum EStopType : byte
{
    /// <summary>
    /// 硬件急停（物理按钮）
    /// </summary>
    Hardware = 0,

    /// <summary>
    /// 软件急停
    /// </summary>
    Software = 1,

    /// <summary>
    /// 远程急停
    /// </summary>
    Remote = 2,

    /// <summary>
    /// 系统失控保护
    /// </summary>
    SystemFailsafe = 3
}
