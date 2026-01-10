using MCCS.Station.Abstractions.Enums;

namespace MCCS.Station.Abstractions.Models;

/// <summary>
/// 命令请求
/// 所有控制命令需要通过 CommandGate 统一过闸
/// </summary>
public sealed class CommandRequest
{
    /// <summary>
    /// 命令ID
    /// </summary>
    public Guid CommandId { get; init; } = Guid.NewGuid();

    /// <summary>
    /// 命令类型
    /// </summary>
    public CommandType Type { get; init; }

    /// <summary>
    /// 命令来源
    /// </summary>
    public string Source { get; init; } = string.Empty;

    /// <summary>
    /// 目标通道（如果适用）
    /// </summary>
    public string? TargetChannel { get; init; }

    /// <summary>
    /// 命令参数
    /// </summary>
    public IReadOnlyDictionary<string, object>? Parameters { get; init; }

    /// <summary>
    /// 需要的能力
    /// </summary>
    public CapabilityFlags RequiredCapabilities { get; init; }

    /// <summary>
    /// 请求时间
    /// </summary>
    public DateTime RequestedAt { get; init; } = DateTime.UtcNow;

    /// <summary>
    /// 优先级
    /// </summary>
    public CommandPriority Priority { get; init; } = CommandPriority.Normal;

    /// <summary>
    /// 是否可以在受限状态下执行
    /// </summary>
    public bool AllowInLimitedState { get; init; }

    /// <summary>
    /// 超时时间（毫秒）
    /// </summary>
    public int TimeoutMs { get; init; } = 5000;
}

/// <summary>
/// 命令类型
/// </summary>
public enum CommandType : byte
{
    // 连接相关
    Connect = 0,
    Disconnect = 1,

    // 能量相关
    Activate = 10,
    Deactivate = 11,
    Pressurize = 12,
    Depressurize = 13,

    // 阀门相关
    OpenValve = 20,
    CloseValve = 21,

    // 控制相关
    StartControl = 30,
    StopControl = 31,
    SetControlMode = 32,
    SetSetpoint = 33,

    // 试验相关
    LoadTest = 40,
    StartTest = 41,
    PauseTest = 42,
    ResumeTest = 43,
    StopTest = 44,
    AbortTest = 45,

    // 手动控制
    ManualMove = 50,
    ManualStop = 51,
    Jog = 52,

    // 安全相关
    ClearInterlock = 60,
    ResetEStop = 61,
    SoftwareEStop = 62,
    ReleaseSoftwareEStop = 63,

    // 标定相关
    Tare = 70,
    Calibrate = 71,

    // 数据相关
    StartRecording = 80,
    StopRecording = 81,

    // 其他
    Custom = 255
}

/// <summary>
/// 命令优先级
/// </summary>
public enum CommandPriority : byte
{
    /// <summary>
    /// 低优先级
    /// </summary>
    Low = 0,

    /// <summary>
    /// 正常优先级
    /// </summary>
    Normal = 1,

    /// <summary>
    /// 高优先级
    /// </summary>
    High = 2,

    /// <summary>
    /// 紧急（安全相关命令）
    /// </summary>
    Emergency = 3
}

/// <summary>
/// 命令执行结果
/// </summary>
public sealed class CommandResult
{
    /// <summary>
    /// 关联的命令ID
    /// </summary>
    public Guid CommandId { get; init; }

    /// <summary>
    /// 是否成功
    /// </summary>
    public bool Success { get; init; }

    /// <summary>
    /// 结果消息
    /// </summary>
    public string Message { get; init; } = string.Empty;

    /// <summary>
    /// 如果被拒绝，拒绝原因
    /// </summary>
    public CommandRejectionReason? RejectionReason { get; init; }

    /// <summary>
    /// 如果被拒绝，缺失的能力
    /// </summary>
    public CapabilityFlags? MissingCapabilities { get; init; }

    /// <summary>
    /// 如果被拒绝，阻止的安全规则
    /// </summary>
    public IReadOnlyList<string>? BlockingRules { get; init; }

    /// <summary>
    /// 返回数据（如果有）
    /// </summary>
    public IReadOnlyDictionary<string, object>? Data { get; init; }

    /// <summary>
    /// 执行时间
    /// </summary>
    public DateTime ExecutedAt { get; init; } = DateTime.UtcNow;

    /// <summary>
    /// 执行耗时（毫秒）
    /// </summary>
    public int DurationMs { get; init; }

    public static CommandResult Succeeded(Guid commandId, string message = "命令执行成功") => new()
    {
        CommandId = commandId,
        Success = true,
        Message = message
    };

    public static CommandResult Failed(Guid commandId, CommandRejectionReason reason, string message,
        CapabilityFlags? missingCapabilities = null, IReadOnlyList<string>? blockingRules = null) => new()
    {
        CommandId = commandId,
        Success = false,
        Message = message,
        RejectionReason = reason,
        MissingCapabilities = missingCapabilities,
        BlockingRules = blockingRules
    };
}

/// <summary>
/// 命令拒绝原因
/// </summary>
public enum CommandRejectionReason : byte
{
    /// <summary>
    /// 能力不足
    /// </summary>
    InsufficientCapabilities = 0,

    /// <summary>
    /// 安全状态不允许
    /// </summary>
    SafetyRestriction = 1,

    /// <summary>
    /// 当前流程状态不允许
    /// </summary>
    InvalidProcessState = 2,

    /// <summary>
    /// 资源不可用
    /// </summary>
    ResourceUnavailable = 3,

    /// <summary>
    /// 联锁阻止
    /// </summary>
    InterlockBlocking = 4,

    /// <summary>
    /// 参数无效
    /// </summary>
    InvalidParameters = 5,

    /// <summary>
    /// 超时
    /// </summary>
    Timeout = 6,

    /// <summary>
    /// 未知错误
    /// </summary>
    Unknown = 255
}
