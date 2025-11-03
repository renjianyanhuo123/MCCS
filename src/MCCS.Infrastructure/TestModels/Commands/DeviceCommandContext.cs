namespace MCCS.Infrastructure.TestModels.Commands
{
    public record DeviceCommandContext
    {
        /// <summary>
        /// 是否有效(用于封装为整体的返回结果; 每次应用一个指令后)
        /// </summary>
        public required bool IsValid { get; set; }

        public long DeviceId { get; init; }
        public int TargetCycleCount { get; init; }
        /// <summary>
        /// 每10个连续数据判断是否达标
        /// </summary>
        public int BufferSize { get; init; } = 10;

        /// <summary>
        /// 用于取消当前指令的订阅流
        /// </summary>
        public IDisposable? CommandSubscribetion { get; set; }

        /// <summary>
        /// 要求的稳定后达到后的条件数据个数
        /// </summary>
        public int RequiredStableCount { get; init; } = 6;
        public float TargetValue { get; init; }
        public float PositionTolerance { get; init; } = 0.5f;
        public bool IsExecuting { get; init; }
        public SystemControlState ControlMode { get; set; }

        private CommandExecuteStatusEnum _currentStatus = CommandExecuteStatusEnum.NoExecute;
        public CommandExecuteStatusEnum CurrentStatus
        {
            get => _currentStatus;
            set
            {
                _currentStatus = value;
                if (_currentStatus == CommandExecuteStatusEnum.ExecuttionCompleted)
                {
                    // 如果有单独开启的监测流;则回收掉
                    CommandSubscribetion?.Dispose();
                }
                StatusChangedEvent?.Invoke();
            }
        }

        public event Action? StatusChangedEvent;
    }
}
