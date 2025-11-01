using MCCS.Infrastructure.Enums;

namespace MCCS.Infrastructure.TestModels.CommandTracking;

/// <summary>
/// 命令执行记录
/// </summary>
public sealed class CommandRecord
{
    /// <summary>
    /// 命令唯一标识
    /// </summary>
    public Guid CommandId { get; init; } = Guid.NewGuid();

    /// <summary>
    /// 控制器ID
    /// </summary>
    public required int ControllerId { get; init; }

    /// <summary>
    /// 设备ID
    /// </summary>
    public required int DeviceId { get; init; }

    /// <summary>
    /// 命令类型（控制模式）
    /// </summary>
    public required ControlMode CommandType { get; init; }

    /// <summary>
    /// 命令参数（存储原始对象）
    /// </summary>
    public required object CommandParams { get; init; }

    /// <summary>
    /// 命令状态
    /// </summary>
    public CommandExecuteStatusEnum Status { get; set; } = CommandExecuteStatusEnum.Idle;

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime CreatedTime { get; init; } = DateTime.Now;

    /// <summary>
    /// 开始执行时间
    /// </summary>
    public DateTime? ExecutionStartTime { get; set; }

    /// <summary>
    /// 执行完成时间
    /// </summary>
    public DateTime? ExecutionCompletedTime { get; set; }

    /// <summary>
    /// 错误信息
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// 执行耗时（毫秒）
    /// </summary>
    public double? ExecutionDurationMs => ExecutionCompletedTime.HasValue && ExecutionStartTime.HasValue
        ? (ExecutionCompletedTime.Value - ExecutionStartTime.Value).TotalMilliseconds
        : null;

    /// <summary>
    /// 命令描述（用于显示）
    /// </summary>
    public string Description => $"{CommandType} - {CreatedTime:HH:mm:ss}";

    /// <summary>
    /// 更新状态
    /// </summary>
    public void UpdateStatus(CommandExecuteStatusEnum newStatus, string? errorMessage = null)
    {
        Status = newStatus;

        switch (newStatus)
        {
            case CommandExecuteStatusEnum.Executing:
                ExecutionStartTime = DateTime.Now;
                break;
            case CommandExecuteStatusEnum.ExecuttionCompleted:
            case CommandExecuteStatusEnum.Error:
                ExecutionCompletedTime = DateTime.Now;
                if (!string.IsNullOrEmpty(errorMessage))
                {
                    ErrorMessage = errorMessage;
                }
                break;
        }
    }
}
