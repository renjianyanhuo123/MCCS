using MCCS.Station.Abstractions.Enums;

namespace MCCS.Station.Abstractions.Models;

/// <summary>
/// 软限位配置
/// 软限位目的：保护试样/过程，系统仍可控
/// </summary>
public sealed class SoftLimitConfig
{
    /// <summary>
    /// 限位ID
    /// </summary>
    public string LimitId { get; init; } = string.Empty;

    /// <summary>
    /// 限位名称
    /// </summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// 关联的通道ID
    /// </summary>
    public string ChannelId { get; init; } = string.Empty;

    /// <summary>
    /// 关联的信号名称
    /// </summary>
    public string SignalName { get; init; } = string.Empty;

    /// <summary>
    /// 限位类型
    /// </summary>
    public SoftLimitType LimitType { get; init; }

    /// <summary>
    /// 上限值
    /// </summary>
    public double? UpperLimit { get; init; }

    /// <summary>
    /// 下限值
    /// </summary>
    public double? LowerLimit { get; init; }

    /// <summary>
    /// 警告阈值百分比（接近限位时预警）
    /// 例如：0.9 表示达到限位值的90%时开始警告
    /// </summary>
    public double WarningThreshold { get; init; } = 0.9;

    /// <summary>
    /// 触发时的动作
    /// </summary>
    public LimitAction TriggerAction { get; init; } = LimitAction.HoldPosition;

    /// <summary>
    /// 返回安全范围后是否自动解除
    /// </summary>
    public bool AutoRelease { get; init; } = true;

    /// <summary>
    /// 解除时需要回到安全值的百分比
    /// 例如：0.85 表示需要回到限位值的85%以下才解除
    /// </summary>
    public double ReleaseThreshold { get; init; } = 0.85;

    /// <summary>
    /// 是否启用
    /// </summary>
    public bool IsEnabled { get; init; } = true;

    /// <summary>
    /// 单位
    /// </summary>
    public string Unit { get; init; } = string.Empty;

    /// <summary>
    /// 优先级
    /// </summary>
    public int Priority { get; init; } = 100;
}

/// <summary>
/// 软限位类型
/// </summary>
public enum SoftLimitType : byte
{
    /// <summary>
    /// 位移限位
    /// </summary>
    Displacement = 0,

    /// <summary>
    /// 力/载荷限位
    /// </summary>
    Load = 1,

    /// <summary>
    /// 速度/速率限位
    /// </summary>
    Velocity = 2,

    /// <summary>
    /// 加速度限位
    /// </summary>
    Acceleration = 3,

    /// <summary>
    /// 应变限位
    /// </summary>
    Strain = 4,

    /// <summary>
    /// 温度限位
    /// </summary>
    Temperature = 5,

    /// <summary>
    /// 同步误差限位
    /// </summary>
    SyncError = 6,

    /// <summary>
    /// 跟随误差限位
    /// </summary>
    FollowError = 7,

    /// <summary>
    /// 控制输出限位
    /// </summary>
    ControlOutput = 8,

    /// <summary>
    /// 自定义限位
    /// </summary>
    Custom = 255
}

/// <summary>
/// 软限位状态（运行时）
/// </summary>
public sealed class SoftLimitState
{
    /// <summary>
    /// 关联的配置
    /// </summary>
    public SoftLimitConfig Config { get; init; } = null!;

    /// <summary>
    /// 当前值
    /// </summary>
    public double CurrentValue { get; init; }

    /// <summary>
    /// 是否处于警告区域
    /// </summary>
    public bool IsWarning { get; init; }

    /// <summary>
    /// 是否已触发
    /// </summary>
    public bool IsTripped { get; init; }

    /// <summary>
    /// 是否触发上限
    /// </summary>
    public bool IsUpperTripped { get; init; }

    /// <summary>
    /// 是否触发下限
    /// </summary>
    public bool IsLowerTripped { get; init; }

    /// <summary>
    /// 触发时间
    /// </summary>
    public DateTime? TrippedAt { get; init; }

    /// <summary>
    /// 触发时的值
    /// </summary>
    public double? TripValue { get; init; }

    /// <summary>
    /// 到上限的余量
    /// </summary>
    public double? MarginToUpper { get; init; }

    /// <summary>
    /// 到下限的余量
    /// </summary>
    public double? MarginToLower { get; init; }

    /// <summary>
    /// 当前执行的动作
    /// </summary>
    public LimitAction? ActiveAction { get; init; }
}
