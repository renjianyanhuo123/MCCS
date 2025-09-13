namespace MCCS.Core.Infrastructure.HardwareDevices
{
    public record DataPoint
    {
        public long DeviceId { get; init; }
        public string SignalId { get; init; }
        public DateTimeOffset Timestamp { get; init; }
        public object Value { get; init; }
        public string Unit { get; init; }  
        public T GetValue<T>() => (T)Convert.ChangeType(Value, typeof(T));
    }
}
