namespace MCCS.Infrastructure.TestModels.Commands;

/// <summary>
/// 设备指令结构
/// </summary>
public record CommandStatusChangeEvent 
{
    public string CommandId { get; } = Guid.NewGuid().ToString();
    /// <summary>
    /// 执行状态
    /// </summary>
    public CommandExecuteStatusEnum Status { get; set; }
    public required long DeviceId { get; init; }
    public CommandTypeEnum Type { get; set; }
    public Dictionary<string, object>? Parameters { get; set; }
    public long Timestamp { get; set; }
    public int TimeoutMs { get; set; } = 5000; // 默认5秒超时
}
