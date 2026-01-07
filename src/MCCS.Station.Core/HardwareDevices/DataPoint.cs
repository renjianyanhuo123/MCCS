namespace MCCS.Station.Core.HardwareDevices
{
    public enum DataQuality : byte
    {
        Good = 0,
        Uncertain = 1,
        Bad = 2
    }

    public record DataPoint<T>
    {
        public long DeviceId { get; init; } 
        public long Timestamp { get; init; }
        public required T Value { get; init; }
        public required string Unit { get; init; }
        public DataQuality DataQuality { get; init; } 
    }

    public record RawSample<T>
    {
        public long DeviceId { get; init; }
        public long SequenceIndex { get; init; }
        public T Value { get; init; }
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
