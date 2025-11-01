using MCCS.Infrastructure.Enums;
using MCCS.Infrastructure.TestModels.CommandTracking;
using System.Collections.ObjectModel;

namespace MCCS.Collecter.Services;

/// <summary>
/// 命令跟踪服务接口
/// </summary>
public interface ICommandTrackingService
{
    /// <summary>
    /// 创建新命令记录
    /// </summary>
    CommandRecord CreateCommand(int controllerId, int deviceId, ControlMode commandType, object commandParams);

    /// <summary>
    /// 更新命令状态
    /// </summary>
    void UpdateCommandStatus(Guid commandId, CommandExecuteStatusEnum status, string? errorMessage = null);

    /// <summary>
    /// 获取命令记录
    /// </summary>
    CommandRecord? GetCommand(Guid commandId);

    /// <summary>
    /// 获取当前正在执行的命令
    /// </summary>
    CommandRecord? GetCurrentExecutingCommand(int controllerId, int deviceId);

    /// <summary>
    /// 获取命令历史（最近N条）
    /// </summary>
    ReadOnlyCollection<CommandRecord> GetCommandHistory(int controllerId, int deviceId, int count = 10);

    /// <summary>
    /// 清理历史记录
    /// </summary>
    void ClearHistory(int controllerId, int deviceId);
}
