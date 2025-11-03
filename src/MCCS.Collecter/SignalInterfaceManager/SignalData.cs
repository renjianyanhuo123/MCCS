namespace MCCS.Collecter.SignalInterfaceManager
{
    /// <summary>
    /// 信号数据
    /// </summary>
    public record SignalData
    {
        /// <summary>
        /// 信号ID
        /// </summary>
        public long SignalId { get; init; }

        /// <summary>
        /// 数据值
        /// </summary>
        public double Value { get; init; }

        /// <summary>
        /// 时间戳
        /// </summary>
        public long Timestamp { get; init; }

        /// <summary>
        /// 数据质量
        /// </summary>
        public bool IsValid { get; init; } = true;
    }
}
