namespace MCCS.Infrastructure.TestModels.Commands
{
    public record DeviceCommandContext
    {
        public long DeviceId { get; init; }
        public int TargetCycleCount { get; init; }

        /// <summary>
        /// 要求的稳定后达到后的条件数据个数
        /// </summary>
        public int RequiredStableCount { get; init; } = 6;
        public float TargetValue { get; init; }
        public float PositionTolerance { get; init; } = 0.5f;
        public bool IsExecuting { get; init; }
        public SystemControlState ControlMode { get; set; }
        public CommandExecuteStatusEnum CurrentStatus { get; set; } = CommandExecuteStatusEnum.NoExecute;
    }
}
