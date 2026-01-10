using MCCS.Station.Abstractions.Enums;
using MCCS.Station.Abstractions.Interfaces;
using MCCS.Station.Abstractions.Models;

namespace MCCS.Station.Core.Safety;

/// <summary>
/// 站点安全上下文
/// 整合所有安全相关组件，提供统一的访问入口
///
/// 架构说明：
/// - StatusAggregator: 四维状态合成器，合成 Connectivity/Activation/Process/Safety
/// - ProcessStateMachine: 只管流程（Idle→Armed→Running…）
/// - SafetySupervisor: 安全主管，整合 LimitEngine/InterlockEngine/EStopMonitor
/// - StationHealthService: 健康服务，从资源树自底向上计算站点状态
/// - CommandGate: 命令过闸，根据能力和安全状态放行/拒绝命令
/// </summary>
public sealed class StationSafetyContext : IDisposable
{
    /// <summary>
    /// 状态合成器
    /// </summary>
    public IStatusAggregator StatusAggregator { get; }

    /// <summary>
    /// 流程状态机
    /// </summary>
    public IProcessStateMachine ProcessStateMachine { get; }

    /// <summary>
    /// 安全主管
    /// </summary>
    public ISafetySupervisor SafetySupervisor { get; }

    /// <summary>
    /// 健康服务
    /// </summary>
    public IStationHealthService HealthService { get; }

    /// <summary>
    /// 命令过闸
    /// </summary>
    public ICommandGate CommandGate { get; }

    /// <summary>
    /// 当前复合状态
    /// </summary>
    public StationCompositeStatus CurrentStatus => StatusAggregator.CurrentStatus;

    /// <summary>
    /// 当前安全状态
    /// </summary>
    public SafetyStatus CurrentSafetyStatus => SafetySupervisor.CurrentSafetyStatus;

    /// <summary>
    /// 当前流程状态
    /// </summary>
    public ProcessStatus CurrentProcessStatus => ProcessStateMachine.CurrentState;

    /// <summary>
    /// 是否可以操作
    /// </summary>
    public bool CanOperate => CurrentStatus.CanOperate;

    /// <summary>
    /// 是否可以启动试验
    /// </summary>
    public bool CanStartTest => CurrentStatus.CanStartTest && ProcessStateMachine.CanStart;

    /// <summary>
    /// 是否需要人工干预
    /// </summary>
    public bool RequiresIntervention => CurrentStatus.RequiresIntervention;

    public StationSafetyContext()
    {
        // 创建状态合成器
        var statusAggregator = new StatusAggregator();
        StatusAggregator = statusAggregator;

        // 创建流程状态机
        ProcessStateMachine = new ProcessStateMachine(statusAggregator);

        // 创建健康服务
        HealthService = new StationHealthService(statusAggregator);

        // 创建安全主管（包含 LimitEngine, InterlockEngine, EStopMonitor）
        SafetySupervisor = new SafetySupervisor(statusAggregator);

        // 创建命令过闸
        CommandGate = new CommandGate(statusAggregator, SafetySupervisor);
    }

    public StationSafetyContext(
        IStatusAggregator statusAggregator,
        IProcessStateMachine processStateMachine,
        ISafetySupervisor safetySupervisor,
        IStationHealthService healthService,
        ICommandGate commandGate)
    {
        StatusAggregator = statusAggregator ?? throw new ArgumentNullException(nameof(statusAggregator));
        ProcessStateMachine = processStateMachine ?? throw new ArgumentNullException(nameof(processStateMachine));
        SafetySupervisor = safetySupervisor ?? throw new ArgumentNullException(nameof(safetySupervisor));
        HealthService = healthService ?? throw new ArgumentNullException(nameof(healthService));
        CommandGate = commandGate ?? throw new ArgumentNullException(nameof(commandGate));
    }

    /// <summary>
    /// 启动安全上下文
    /// </summary>
    public async Task StartAsync(CancellationToken cancellationToken = default)
    {
        Console.WriteLine("[StationSafetyContext] 启动安全上下文...");

        await HealthService.StartAsync(cancellationToken);
        await SafetySupervisor.StartAsync(cancellationToken);

        Console.WriteLine("[StationSafetyContext] 安全上下文已启动");
    }

    /// <summary>
    /// 停止安全上下文
    /// </summary>
    public async Task StopAsync()
    {
        Console.WriteLine("[StationSafetyContext] 停止安全上下文...");

        await SafetySupervisor.StopAsync();
        await HealthService.StopAsync();

        Console.WriteLine("[StationSafetyContext] 安全上下文已停止");
    }

    /// <summary>
    /// 检查并执行命令
    /// </summary>
    public async Task<CommandResult> ExecuteCommandAsync(
        CommandRequest request,
        Func<CommandRequest, CancellationToken, Task<CommandResult>> executor,
        CancellationToken cancellationToken = default)
    {
        return await CommandGate.SubmitAsync(request, executor, cancellationToken);
    }

    /// <summary>
    /// 触发软件急停
    /// </summary>
    public async Task TriggerEmergencyStopAsync(string reason, string triggeredBy)
    {
        await SafetySupervisor.TriggerSoftwareEStopAsync(reason, triggeredBy);

        // 强制流程状态机进入安全状态
        if (ProcessStateMachine.IsRunning)
        {
            ProcessStateMachine.ForceTransition(ProcessStatus.Stopping, "急停触发", "SafetySystem");
        }
    }

    /// <summary>
    /// 尝试复位系统
    /// </summary>
    public async Task<bool> TryResetAsync(string operatorId, string reason)
    {
        // 1. 复位急停
        var estopReset = await SafetySupervisor.TryResetEStopAsync(operatorId, reason);
        if (!estopReset)
        {
            return false;
        }

        // 2. 清除可自动恢复的问题
        StatusAggregator.ClearAutoRecoverableIssues();

        // 3. 刷新状态
        await StatusAggregator.RefreshAsync();

        return true;
    }

    /// <summary>
    /// 获取状态摘要
    /// </summary>
    public string GetStatusSummary()
    {
        var status = CurrentStatus;
        return $"""
            站点状态摘要
            ============
            连接状态: {status.Connectivity}
            激活状态: {status.Activation}
            流程状态: {status.Process}
            安全状态: {status.Safety}
            可用能力: {status.Capabilities}
            摘要: {status.Summary}
            活跃问题: {status.ActiveIssues.Count} 个
            """;
    }

    /// <summary>
    /// 注册默认的联锁规则
    /// </summary>
    public void RegisterDefaultInterlocks()
    {
        var interlockEngine = SafetySupervisor.InterlockEngine;

        // 急停联锁
        interlockEngine.RegisterRule(new InterlockRule
        {
            RuleId = "ESTOP_001",
            Name = "急停按钮",
            Type = InterlockTypeEnum.EStop,
            Description = "急停按钮被按下",
            SourceSignals = new[] { "estop" },
            ConditionExpression = "estop != 0",
            IsLatched = true,
            ResetPolicy = InterlockResetPolicy.Hardware,
            DisabledCapabilities = CapabilityFlags.Full & ~CapabilityFlags.CanResetEStop,
            TriggerAction = InterlockAction.CutPower,
            Priority = 1000
        });

        // 门禁联锁
        interlockEngine.RegisterRule(new InterlockRule
        {
            RuleId = "DOOR_001",
            Name = "安全门",
            Type = InterlockTypeEnum.DoorOpen,
            Description = "安全门未关闭",
            SourceSignals = new[] { "door_closed" },
            ConditionExpression = "door_closed == 0",
            IsLatched = false,
            ResetPolicy = InterlockResetPolicy.Auto,
            DisabledCapabilities = CapabilityFlags.CanStartTest | CapabilityFlags.CanMove,
            TriggerAction = InterlockAction.Hold,
            Priority = 900
        });

        // 油压联锁
        interlockEngine.RegisterRule(new InterlockRule
        {
            RuleId = "OIL_PRESSURE_001",
            Name = "油压过低",
            Type = InterlockTypeEnum.OilPressure,
            Description = "油压低于安全阈值",
            SourceSignals = new[] { "oil_pressure" },
            ConditionExpression = "oil_pressure < 100",
            IsLatched = true,
            ResetPolicy = InterlockResetPolicy.Manual,
            DisabledCapabilities = CapabilityFlags.CanControl | CapabilityFlags.CanMove,
            TriggerAction = InterlockAction.CloseValve,
            Priority = 800
        });

        // 油温联锁
        interlockEngine.RegisterRule(new InterlockRule
        {
            RuleId = "OIL_TEMP_001",
            Name = "油温过高",
            Type = InterlockTypeEnum.OilTemperature,
            Description = "油温超过安全阈值",
            SourceSignals = new[] { "oil_temperature" },
            ConditionExpression = "oil_temperature > 60",
            IsLatched = true,
            ResetPolicy = InterlockResetPolicy.Manual,
            DisabledCapabilities = CapabilityFlags.CanPressurize,
            TriggerAction = InterlockAction.Hold,
            Priority = 700
        });

        Console.WriteLine("[StationSafetyContext] 已注册默认联锁规则");
    }

    /// <summary>
    /// 注册默认的软限位
    /// </summary>
    public void RegisterDefaultLimits(string channelId, double displacementMax, double loadMax)
    {
        var limitEngine = SafetySupervisor.LimitEngine;

        // 位移上限
        limitEngine.RegisterLimit(new SoftLimitConfig
        {
            LimitId = $"{channelId}_DISP_UPPER",
            Name = "位移上限",
            ChannelId = channelId,
            SignalName = "displacement",
            LimitType = SoftLimitType.Displacement,
            UpperLimit = displacementMax,
            WarningThreshold = 0.9,
            TriggerAction = LimitAction.HoldPosition,
            AutoRelease = true,
            ReleaseThreshold = 0.85,
            Unit = "mm"
        });

        // 位移下限
        limitEngine.RegisterLimit(new SoftLimitConfig
        {
            LimitId = $"{channelId}_DISP_LOWER",
            Name = "位移下限",
            ChannelId = channelId,
            SignalName = "displacement",
            LimitType = SoftLimitType.Displacement,
            LowerLimit = -displacementMax,
            WarningThreshold = 0.9,
            TriggerAction = LimitAction.HoldPosition,
            AutoRelease = true,
            ReleaseThreshold = 0.85,
            Unit = "mm"
        });

        // 载荷上限
        limitEngine.RegisterLimit(new SoftLimitConfig
        {
            LimitId = $"{channelId}_LOAD_UPPER",
            Name = "载荷上限",
            ChannelId = channelId,
            SignalName = "load",
            LimitType = SoftLimitType.Load,
            UpperLimit = loadMax,
            WarningThreshold = 0.9,
            TriggerAction = LimitAction.HoldLoad,
            AutoRelease = true,
            ReleaseThreshold = 0.85,
            Unit = "kN"
        });

        // 载荷下限
        limitEngine.RegisterLimit(new SoftLimitConfig
        {
            LimitId = $"{channelId}_LOAD_LOWER",
            Name = "载荷下限",
            ChannelId = channelId,
            SignalName = "load",
            LimitType = SoftLimitType.Load,
            LowerLimit = -loadMax,
            WarningThreshold = 0.9,
            TriggerAction = LimitAction.HoldLoad,
            AutoRelease = true,
            ReleaseThreshold = 0.85,
            Unit = "kN"
        });

        Console.WriteLine($"[StationSafetyContext] 已为通道 {channelId} 注册默认软限位");
    }

    public void Dispose()
    {
        if (StatusAggregator is IDisposable statusDisposable)
        {
            statusDisposable.Dispose();
        }

        if (ProcessStateMachine is IDisposable processDisposable)
        {
            processDisposable.Dispose();
        }

        if (SafetySupervisor is IDisposable safetyDisposable)
        {
            safetyDisposable.Dispose();
        }

        if (HealthService is IDisposable healthDisposable)
        {
            healthDisposable.Dispose();
        }

        if (CommandGate is IDisposable commandDisposable)
        {
            commandDisposable.Dispose();
        }
    }
}
