namespace MCCS.Infrastructure.TestModels.Commands
{
    /// <summary>
    /// 命令状态变化事件
    /// </summary>
    public class CommandStatusChangeEvent
    {
        /// <summary>
        /// 设备ID（通道ID）
        /// </summary>
        public long DeviceId { get; set; }

        /// <summary>
        /// 命令执行状态
        /// </summary>
        public CommandExecuteStatusEnum Status { get; set; }

        /// <summary>
        /// 状态变化时间戳
        /// </summary>
        public long Timestamp { get; set; }
    }
}
