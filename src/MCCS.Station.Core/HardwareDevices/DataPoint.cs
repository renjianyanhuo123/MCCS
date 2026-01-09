namespace MCCS.Station.Core.HardwareDevices
{ 
    public record DataPoint<T>
    {
        public long SequenceIndex { get; init; }
        public long DeviceId { get; init; } 
        public long Timestamp { get; init; }
        public required T Value { get; init; }
        public required string Unit { get; init; }
    } 

    /// <summary>
    /// 采集层批量采集使用
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public sealed record SampleBatch<T>
    {
        public long DeviceId { get; init; }
        public long SequenceStart { get; init; }
        public long SampleCount { get; init; }
        /// <summary>
        /// 锚点
        /// </summary>
        public long ArrivalTicks { get; init; }
        public T[] Values { get; set; } 
    }
}
