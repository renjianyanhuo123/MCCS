using MCCS.Station.Abstractions.Enums;
using MCCS.Station.Abstractions.Events;
using MCCS.Station.Abstractions.Models;

namespace MCCS.Station.Abstractions.Interfaces;

/// <summary>
/// 命令过闸接口
/// 所有控制命令统一过闸，根据 Capabilities + Safety 状态放行/改写/拒绝
/// </summary>
public interface ICommandGate
{
    /// <summary>
    /// 命令过闸事件流（用于审计和日志）
    /// </summary>
    IObservable<CommandGateEvent> CommandProcessed { get; }

    /// <summary>
    /// 检查命令是否可以执行（不实际执行）
    /// </summary>
    CommandCheckResult CheckCommand(CommandRequest request);

    /// <summary>
    /// 提交命令请求
    /// </summary>
    /// <param name="request">命令请求</param>
    /// <param name="executor">实际执行命令的委托</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>命令执行结果</returns>
    Task<CommandResult> SubmitAsync(
        CommandRequest request,
        Func<CommandRequest, CancellationToken, Task<CommandResult>> executor,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 提交紧急命令（跳过部分检查）
    /// </summary>
    /// <param name="request">命令请求</param>
    /// <param name="executor">实际执行命令的委托</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>命令执行结果</returns>
    Task<CommandResult> SubmitEmergencyAsync(
        CommandRequest request,
        Func<CommandRequest, CancellationToken, Task<CommandResult>> executor,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 注册命令拦截器
    /// </summary>
    void RegisterInterceptor(ICommandInterceptor interceptor, int priority = 0);

    /// <summary>
    /// 移除命令拦截器
    /// </summary>
    void RemoveInterceptor(ICommandInterceptor interceptor);

    /// <summary>
    /// 获取指定命令类型需要的能力
    /// </summary>
    CapabilityFlags GetRequiredCapabilities(CommandType commandType);

    /// <summary>
    /// 获取当前可执行的命令类型
    /// </summary>
    IReadOnlyList<CommandType> GetAvailableCommands();

    /// <summary>
    /// 获取被阻止的命令类型及原因
    /// </summary>
    IReadOnlyDictionary<CommandType, string> GetBlockedCommands();

    /// <summary>
    /// 获取等待中的命令数量
    /// </summary>
    int PendingCommandCount { get; }

    /// <summary>
    /// 获取命令统计
    /// </summary>
    CommandGateStatistics GetStatistics();
}

/// <summary>
/// 命令检查结果
/// </summary>
public sealed class CommandCheckResult
{
    /// <summary>
    /// 是否允许执行
    /// </summary>
    public bool Allowed { get; init; }

    /// <summary>
    /// 拒绝原因
    /// </summary>
    public CommandRejectionReason? RejectionReason { get; init; }

    /// <summary>
    /// 拒绝详情
    /// </summary>
    public string RejectionDetails { get; init; } = string.Empty;

    /// <summary>
    /// 缺失的能力
    /// </summary>
    public CapabilityFlags? MissingCapabilities { get; init; }

    /// <summary>
    /// 阻止的规则
    /// </summary>
    public IReadOnlyList<string>? BlockingRules { get; init; }

    /// <summary>
    /// 命令是否需要修改
    /// </summary>
    public bool RequiresModification { get; init; }

    /// <summary>
    /// 修改后的命令（如果需要修改）
    /// </summary>
    public CommandRequest? ModifiedRequest { get; init; }

    /// <summary>
    /// 警告信息（命令可以执行但有警告）
    /// </summary>
    public IReadOnlyList<string>? Warnings { get; init; }

    public static CommandCheckResult Pass() => new() { Allowed = true };

    public static CommandCheckResult Reject(CommandRejectionReason reason, string details,
        CapabilityFlags? missingCapabilities = null, IReadOnlyList<string>? blockingRules = null) => new()
    {
        Allowed = false,
        RejectionReason = reason,
        RejectionDetails = details,
        MissingCapabilities = missingCapabilities,
        BlockingRules = blockingRules
    };
}

/// <summary>
/// 命令拦截器接口
/// </summary>
public interface ICommandInterceptor
{
    /// <summary>
    /// 拦截器名称
    /// </summary>
    string Name { get; }

    /// <summary>
    /// 拦截命令
    /// </summary>
    /// <param name="request">原始命令请求</param>
    /// <param name="context">拦截上下文</param>
    /// <returns>拦截结果</returns>
    Task<CommandInterceptResult> InterceptAsync(CommandRequest request, CommandInterceptContext context);
}

/// <summary>
/// 命令拦截上下文
/// </summary>
public sealed class CommandInterceptContext
{
    /// <summary>
    /// 当前复合状态
    /// </summary>
    public StationCompositeStatus CurrentStatus { get; init; } = null!;

    /// <summary>
    /// 可用能力
    /// </summary>
    public CapabilityFlags AvailableCapabilities { get; init; }

    /// <summary>
    /// 额外数据
    /// </summary>
    public IReadOnlyDictionary<string, object>? AdditionalData { get; init; }
}

/// <summary>
/// 命令拦截结果
/// </summary>
public sealed class CommandInterceptResult
{
    /// <summary>
    /// 是否继续执行
    /// </summary>
    public bool Continue { get; init; } = true;

    /// <summary>
    /// 是否修改了命令
    /// </summary>
    public bool Modified { get; init; }

    /// <summary>
    /// 修改后的命令
    /// </summary>
    public CommandRequest? ModifiedRequest { get; init; }

    /// <summary>
    /// 拒绝原因（如果Continue=false）
    /// </summary>
    public string? RejectionReason { get; init; }

    public static CommandInterceptResult Pass() => new() { Continue = true };

    public static CommandInterceptResult Modify(CommandRequest modifiedRequest) => new()
    {
        Continue = true,
        Modified = true,
        ModifiedRequest = modifiedRequest
    };

    public static CommandInterceptResult Reject(string reason) => new()
    {
        Continue = false,
        RejectionReason = reason
    };
}

/// <summary>
/// 命令过闸统计
/// </summary>
public sealed class CommandGateStatistics
{
    public long TotalCommands { get; init; }
    public long PassedCommands { get; init; }
    public long RejectedCommands { get; init; }
    public long ModifiedCommands { get; init; }
    public long EmergencyCommands { get; init; }
    public IReadOnlyDictionary<CommandType, long> CommandsByType { get; init; } = new Dictionary<CommandType, long>();
    public IReadOnlyDictionary<CommandRejectionReason, long> RejectionsByReason { get; init; } = new Dictionary<CommandRejectionReason, long>();
    public DateTime StartTime { get; init; }
    public TimeSpan Uptime => DateTime.UtcNow - StartTime;
}
