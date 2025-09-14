namespace MCCS.Collecter.HardwareDevices
{
    public record HardwareDeviceConfiguration
    {
        public long DeviceId { get; init; } = 0;                         // 设备ID
        public string DeviceName { get; init; } = string.Empty;          // 设备名称
        public string DeviceType { get; init; } = string.Empty;          // 设备类型
        public string ConnectionString { get; init; } = string.Empty;    // 连接字符串或地址
        public List<HardwareSignalConfiguration> Signals { get; init; } = [];
    }
}
