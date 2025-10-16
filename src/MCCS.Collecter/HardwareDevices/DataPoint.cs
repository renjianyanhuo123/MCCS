namespace MCCS.Collecter.HardwareDevices
{
    public enum DataQuality : byte
    {
        Good = 0,
        Uncertain = 1,
        Bad = 2
    }

    public record DataPoint
    {
        public int DeviceId { get; init; } 
        public long Timestamp { get; init; }
        public object Value { get; init; }
        public string Unit { get; init; }
        public DataQuality DataQuality { get; init; }
        public T GetValue<T>() => (T)Convert.ChangeType(Value, typeof(T));
    }
}
