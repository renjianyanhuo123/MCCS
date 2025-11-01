using MCCS.Events.Controllers;
using MCCS.Infrastructure.Enums;
using MCCS.Infrastructure.TestModels.CommandTracking;
using Prism.Events;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;

namespace MCCS.Collecter.Services;

/// <summary>
/// 命令跟踪服务实现
/// </summary>
public class CommandTrackingService : ICommandTrackingService
{
    private readonly IEventAggregator _eventAggregator;

    // 使用线程安全的字典存储命令记录
    // Key: CommandId
    private readonly ConcurrentDictionary<Guid, CommandRecord> _commands = new();

    // 按控制器和设备分组的命令历史
    // Key: "ControllerId_DeviceId"
    private readonly ConcurrentDictionary<string, List<CommandRecord>> _commandHistory = new();

    public CommandTrackingService(IEventAggregator eventAggregator)
    {
        _eventAggregator = eventAggregator;
    }

    public CommandRecord CreateCommand(int controllerId, int deviceId, ControlMode commandType, object commandParams)
    {
        var command = new CommandRecord
        {
            ControllerId = controllerId,
            DeviceId = deviceId,
            CommandType = commandType,
            CommandParams = commandParams
        };

        _commands[command.CommandId] = command;

        // 添加到历史记录
        var key = GetHistoryKey(controllerId, deviceId);
        _commandHistory.AddOrUpdate(
            key,
            _ => new List<CommandRecord> { command },
            (_, list) =>
            {
                list.Add(command);
                // 保留最近100条记录
                if (list.Count > 100)
                {
                    list.RemoveRange(0, list.Count - 100);
                }
                return list;
            });

        return command;
    }

    public void UpdateCommandStatus(Guid commandId, CommandExecuteStatusEnum status, string? errorMessage = null)
    {
        if (_commands.TryGetValue(commandId, out var command))
        {
            command.UpdateStatus(status, errorMessage);

            // 发布状态变更事件
            _eventAggregator.GetEvent<CommandStatusChangedEvent>().Publish(new CommandStatusChangedEventParam
            {
                CommandRecord = command
            });
        }
    }

    public CommandRecord? GetCommand(Guid commandId)
    {
        return _commands.TryGetValue(commandId, out var command) ? command : null;
    }

    public CommandRecord? GetCurrentExecutingCommand(int controllerId, int deviceId)
    {
        var key = GetHistoryKey(controllerId, deviceId);

        if (_commandHistory.TryGetValue(key, out var history))
        {
            return history
                .Where(c => c.Status == CommandExecuteStatusEnum.Executing ||
                           c.Status == CommandExecuteStatusEnum.Stoping)
                .OrderByDescending(c => c.CreatedTime)
                .FirstOrDefault();
        }

        return null;
    }

    public ReadOnlyCollection<CommandRecord> GetCommandHistory(int controllerId, int deviceId, int count = 10)
    {
        var key = GetHistoryKey(controllerId, deviceId);

        if (_commandHistory.TryGetValue(key, out var history))
        {
            var records = history
                .OrderByDescending(c => c.CreatedTime)
                .Take(count)
                .ToList();

            return new ReadOnlyCollection<CommandRecord>(records);
        }

        return new ReadOnlyCollection<CommandRecord>(new List<CommandRecord>());
    }

    public void ClearHistory(int controllerId, int deviceId)
    {
        var key = GetHistoryKey(controllerId, deviceId);
        _commandHistory.TryRemove(key, out _);
    }

    private static string GetHistoryKey(int controllerId, int deviceId) => $"{controllerId}_{deviceId}";
}
