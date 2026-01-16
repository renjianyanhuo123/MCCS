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
    /// 注意：为支持对象池复用，属性设计为可变的
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public sealed class SampleBatch<T>
    {
        public long DeviceId { get; set; }
        public long SequenceStart { get; set; }
        public long SampleCount { get; set; }
        /// <summary>
        /// 锚点
        /// </summary>
        public long ArrivalTicks { get; set; }
        public T[]? Values { get; set; }
    }
}
