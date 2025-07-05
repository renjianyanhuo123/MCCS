namespace MCCS.Core.Devices
{
    /// <summary>
    /// 统一的设备数据结构
    /// </summary>
    public record DeviceData
    { 
        public required string DeviceId { get; init; }
        public required object Value { get; init; }
        public string? Unit { get; init; }
        public DateTimeOffset Timestamp { get; init; }
        public Dictionary<string, object>? Metadata { get; init; }
    }
}
