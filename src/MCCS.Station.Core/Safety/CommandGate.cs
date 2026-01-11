using System.Collections.Concurrent;
using System.Reactive.Subjects;
using MCCS.Station.Abstractions.Enums;
using MCCS.Station.Abstractions.Events;
using MCCS.Station.Abstractions.Interfaces;
using MCCS.Station.Abstractions.Models;

namespace MCCS.Station.Core.Safety;

/// <summary>
/// 命令过闸
/// 所有控制命令统一过闸，根据 Capabilities + Safety 状态放行/改写/拒绝
/// </summary>
public sealed class CommandGate : ICommandGate, IDisposable
{
    private readonly object _lock = new();
    private readonly Subject<CommandGateEvent> _commandProcessed = new();
    private readonly IStatusAggregator _statusAggregator;
    private readonly ISafetySupervisor _safetySupervisor;
    private readonly SortedList<int, ICommandInterceptor> _interceptors = new();
    private readonly ConcurrentQueue<CommandRequest> _pendingCommands = new();

    private readonly DateTime _startTime = DateTime.UtcNow;
    private long _totalCommands;
    private long _passedCommands;
    private long _rejectedCommands;
    private long _modifiedCommands;
    private long _emergencyCommands;
    private readonly ConcurrentDictionary<CommandType, long> _commandsByType = new();
    private readonly ConcurrentDictionary<CommandRejectionReason, long> _rejectionsByReason = new();

    // 命令类型到所需能力的映射
    // ReSharper disable once InconsistentNaming
    private static readonly Dictionary<CommandType, CapabilityFlags> CommandCapabilityMap = new()
    {
        [CommandType.Connect] = CapabilityFlags.CanConnect,
        [CommandType.Disconnect] = CapabilityFlags.CanConnect,
        [CommandType.Activate] = CapabilityFlags.CanActivate,
        [CommandType.Deactivate] = CapabilityFlags.CanActivate,
        [CommandType.Pressurize] = CapabilityFlags.CanPressurize,
        [CommandType.Depressurize] = CapabilityFlags.CanPressurize,
        [CommandType.OpenValve] = CapabilityFlags.CanActivate,
        [CommandType.CloseValve] = CapabilityFlags.CanActivate,
        [CommandType.StartControl] = CapabilityFlags.CanControl,
        [CommandType.StopControl] = CapabilityFlags.CanControl,
        [CommandType.SetControlMode] = CapabilityFlags.CanControl,
        [CommandType.SetSetpoint] = CapabilityFlags.CanControl,
        [CommandType.LoadTest] = CapabilityFlags.CanModifyParameters,
        [CommandType.StartTest] = CapabilityFlags.CanStartTest,
        [CommandType.PauseTest] = CapabilityFlags.CanPause,
        [CommandType.ResumeTest] = CapabilityFlags.CanResume,
        [CommandType.StopTest] = CapabilityFlags.CanStop,
        [CommandType.AbortTest] = CapabilityFlags.CanStop,
        [CommandType.ManualMove] = CapabilityFlags.CanManualControl,
        [CommandType.ManualStop] = CapabilityFlags.CanManualControl,
        [CommandType.Jog] = CapabilityFlags.CanManualControl,
        [CommandType.ClearInterlock] = CapabilityFlags.CanClearInterlock,
        [CommandType.ResetEStop] = CapabilityFlags.CanResetEStop,
        [CommandType.SoftwareEStop] = CapabilityFlags.CanResetEStop,
        [CommandType.ReleaseSoftwareEStop] = CapabilityFlags.CanResetEStop,
        [CommandType.Tare] = CapabilityFlags.CanTare,
        [CommandType.Calibrate] = CapabilityFlags.CanCalibrate,
        [CommandType.StartRecording] = CapabilityFlags.CanRecord,
        [CommandType.StopRecording] = CapabilityFlags.CanRecord
    };

    // 紧急命令类型（可以在受限状态下执行）
    // ReSharper disable once InconsistentNaming
    private static readonly HashSet<CommandType> EmergencyCommandTypes =
    [
        CommandType.ClearInterlock,
        CommandType.ResetEStop,
        CommandType.SoftwareEStop,
        CommandType.ReleaseSoftwareEStop,
        CommandType.StopTest,
        CommandType.AbortTest,
        CommandType.ManualStop,
        CommandType.CloseValve,
        CommandType.Deactivate
    ];

    public CommandGate(IStatusAggregator statusAggregator, ISafetySupervisor safetySupervisor)
    {
        _statusAggregator = statusAggregator ?? throw new ArgumentNullException(nameof(statusAggregator));
        _safetySupervisor = safetySupervisor ?? throw new ArgumentNullException(nameof(safetySupervisor));
    }

    public IObservable<CommandGateEvent> CommandProcessed => _commandProcessed;

    public int PendingCommandCount => _pendingCommands.Count;

    public CommandCheckResult CheckCommand(CommandRequest request)
    {
        var currentStatus = _statusAggregator.CurrentStatus;
        var requiredCaps = GetRequiredCapabilities(request.Type);

        // 1. 检查是否是紧急命令
        if (EmergencyCommandTypes.Contains(request.Type))
        {
            return CommandCheckResult.Pass();
        }

        // 2. 检查安全状态
        if (currentStatus.Safety >= SafetyStatus.Interlocked && !request.AllowInLimitedState)
        {
            (var allowed, var reason, IReadOnlyList<string> blockingRules) = _safetySupervisor.CheckOperation(requiredCaps);
            if (!allowed)
            {
                return CommandCheckResult.Reject(
                    CommandRejectionReason.SafetyRestriction,
                    reason,
                    blockingRules: blockingRules);
            }
        }

        // 3. 检查能力
        var availableCaps = currentStatus.Capabilities;
        var missingCaps = requiredCaps & ~availableCaps;

        if (missingCaps != CapabilityFlags.None)
        {
            return CommandCheckResult.Reject(
                CommandRejectionReason.InsufficientCapabilities,
                $"缺少所需能力: {missingCaps}",
                missingCapabilities: missingCaps);
        }

        // 4. 运行拦截器
        var context = new CommandInterceptContext
        {
            CurrentStatus = currentStatus,
            AvailableCapabilities = availableCaps
        };

        foreach (var interceptor in _interceptors.Values)
        {
            try
            {
                var interceptResult = interceptor.InterceptAsync(request, context).GetAwaiter().GetResult();

                if (!interceptResult.Continue)
                {
                    return CommandCheckResult.Reject(
                        CommandRejectionReason.SafetyRestriction,
                        interceptResult.RejectionReason ?? "被拦截器拒绝");
                }

                if (interceptResult.Modified && interceptResult.ModifiedRequest != null)
                {
                    return new CommandCheckResult
                    {
                        Allowed = true,
                        RequiresModification = true,
                        ModifiedRequest = interceptResult.ModifiedRequest
                    };
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CommandGate] 拦截器 {interceptor.Name} 异常: {ex.Message}");
            }
        }

        return CommandCheckResult.Pass();
    }

    public async Task<CommandResult> SubmitAsync(
        CommandRequest request,
        Func<CommandRequest, CancellationToken, Task<CommandResult>> executor,
        CancellationToken cancellationToken = default)
    {
        var sw = System.Diagnostics.Stopwatch.StartNew();
        Interlocked.Increment(ref _totalCommands);
        IncrementCommandType(request.Type);

        try
        {
            // 检查命令
            var checkResult = CheckCommand(request);

            if (!checkResult.Allowed)
            {
                Interlocked.Increment(ref _rejectedCommands);
                IncrementRejectionReason(checkResult.RejectionReason!.Value);

                PublishEvent(request, false, checkResult.RejectionReason, checkResult.RejectionDetails,
                    checkResult.BlockingRules, checkResult.MissingCapabilities);

                return CommandResult.Failed(
                    request.CommandId,
                    checkResult.RejectionReason!.Value,
                    checkResult.RejectionDetails,
                    checkResult.MissingCapabilities,
                    checkResult.BlockingRules);
            }

            // 如果需要修改请求
            var effectiveRequest = checkResult.RequiresModification && checkResult.ModifiedRequest != null
                ? checkResult.ModifiedRequest
                : request;

            if (checkResult.RequiresModification)
            {
                Interlocked.Increment(ref _modifiedCommands);
            }

            // 执行命令
            var result = await executor(effectiveRequest, cancellationToken);

            if (result.Success)
            {
                Interlocked.Increment(ref _passedCommands);
            }
            else
            {
                Interlocked.Increment(ref _rejectedCommands);
            }

            sw.Stop();
            PublishEvent(request, result.Success, null, result.Message, null, null);

            return result with { DurationMs = (int)sw.ElapsedMilliseconds };
        }
        catch (OperationCanceledException)
        {
            Interlocked.Increment(ref _rejectedCommands);
            IncrementRejectionReason(CommandRejectionReason.Timeout);

            return CommandResult.Failed(
                request.CommandId,
                CommandRejectionReason.Timeout,
                "命令执行超时或被取消");
        }
        catch (Exception ex)
        {
            Interlocked.Increment(ref _rejectedCommands);
            IncrementRejectionReason(CommandRejectionReason.Unknown);

            return CommandResult.Failed(
                request.CommandId,
                CommandRejectionReason.Unknown,
                $"命令执行异常: {ex.Message}");
        }
    }

    public async Task<CommandResult> SubmitEmergencyAsync(
        CommandRequest request,
        Func<CommandRequest, CancellationToken, Task<CommandResult>> executor,
        CancellationToken cancellationToken = default)
    {
        Interlocked.Increment(ref _emergencyCommands);
        Console.WriteLine($"[CommandGate] 紧急命令: {request.Type} ({request.CommandId})");

        // 紧急命令跳过大部分检查，直接执行
        var result = await executor(request, cancellationToken);

        PublishEvent(request, result.Success, null, $"紧急命令: {result.Message}", null, null);

        return result;
    }

    public void RegisterInterceptor(ICommandInterceptor interceptor, int priority = 0)
    {
        lock (_lock)
        {
            // 确保优先级唯一
            while (_interceptors.ContainsKey(priority))
            {
                priority++;
            }
            _interceptors.Add(priority, interceptor);
            Console.WriteLine($"[CommandGate] 注册拦截器: {interceptor.Name} (优先级: {priority})");
        }
    }

    public void RemoveInterceptor(ICommandInterceptor interceptor)
    {
        lock (_lock)
        {
            var key = _interceptors.FirstOrDefault(kv => kv.Value == interceptor).Key;
            if (_interceptors.ContainsKey(key))
            {
                _interceptors.Remove(key);
                Console.WriteLine($"[CommandGate] 移除拦截器: {interceptor.Name}");
            }
        }
    }

    public CapabilityFlags GetRequiredCapabilities(CommandType commandType)
    {
        return CommandCapabilityMap.TryGetValue(commandType, out var caps)
            ? caps
            : CapabilityFlags.None;
    }

    public IReadOnlyList<CommandType> GetAvailableCommands()
    {
        var available = new List<CommandType>();
        var currentCaps = _statusAggregator.CurrentStatus.Capabilities;

        foreach (var (commandType, requiredCaps) in CommandCapabilityMap)
        {
            if ((currentCaps & requiredCaps) == requiredCaps)
            {
                available.Add(commandType);
            }
        }

        return available.AsReadOnly();
    }

    public IReadOnlyDictionary<CommandType, string> GetBlockedCommands()
    {
        var blocked = new Dictionary<CommandType, string>();
        var currentCaps = _statusAggregator.CurrentStatus.Capabilities;
        var currentStatus = _statusAggregator.CurrentStatus;

        foreach (var (commandType, requiredCaps) in CommandCapabilityMap)
        {
            var missingCaps = requiredCaps & ~currentCaps;
            if (missingCaps != CapabilityFlags.None)
            {
                blocked[commandType] = $"缺少能力: {missingCaps}";
            }
            else if (currentStatus.Safety >= SafetyStatus.Interlocked && !EmergencyCommandTypes.Contains(commandType))
            {
                blocked[commandType] = $"安全状态限制: {currentStatus.Safety}";
            }
        }

        return blocked;
    }

    public CommandGateStatistics GetStatistics()
    {
        return new CommandGateStatistics
        {
            TotalCommands = Interlocked.Read(ref _totalCommands),
            PassedCommands = Interlocked.Read(ref _passedCommands),
            RejectedCommands = Interlocked.Read(ref _rejectedCommands),
            ModifiedCommands = Interlocked.Read(ref _modifiedCommands),
            EmergencyCommands = Interlocked.Read(ref _emergencyCommands),
            CommandsByType = _commandsByType.ToDictionary(kv => kv.Key, kv => kv.Value),
            RejectionsByReason = _rejectionsByReason.ToDictionary(kv => kv.Key, kv => kv.Value),
            StartTime = _startTime
        };
    }

    private void IncrementCommandType(CommandType type)
    {
        _commandsByType.AddOrUpdate(type, 1, (_, count) => count + 1);
    }

    private void IncrementRejectionReason(CommandRejectionReason reason)
    {
        _rejectionsByReason.AddOrUpdate(reason, 1, (_, count) => count + 1);
    }

    private void PublishEvent(CommandRequest request, bool passed, CommandRejectionReason? rejectionReason,
        string details, IReadOnlyList<string>? blockingRules, CapabilityFlags? missingCapabilities)
    {
        var evt = new CommandGateEvent
        {
            CommandId = request.CommandId,
            CommandType = request.Type,
            CommandSource = request.Source,
            TargetChannel = request.TargetChannel,
            Passed = passed,
            RejectionReason = rejectionReason,
            RejectionDetails = details,
            BlockingRules = blockingRules,
            MissingCapabilities = missingCapabilities,
            SafetyStatus = _statusAggregator.CurrentStatus.Safety,
            Source = "CommandGate"
        };

        _commandProcessed.OnNext(evt);
    }

    public void Dispose()
    {
        _commandProcessed.OnCompleted();
        _commandProcessed.Dispose();
    }
}
