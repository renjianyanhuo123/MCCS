namespace MCCS.Collecter.HardwareDevices
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
        public T Value { get; init; }
        public string Unit { get; init; }
        public DataQuality DataQuality { get; init; } 
    }
}
