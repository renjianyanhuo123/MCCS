namespace MCCS.Collecter.HardwareDevices
{
    public record HardwareDeviceConfiguration
    {
        public long DeviceId { get; init; } = 0;                         // 设备ID
        public int DeviceAddressId { get; init; }                        // 设备地址ID(0.......n 编号)  准确说: 应该叫控制器编号
        public string DeviceName { get; init; } = string.Empty;          // 设备名称
        public string DeviceType { get; init; } = string.Empty;          // 设备类型
        public string ConnectionString { get; init; } = string.Empty;    // 连接字符串或地址
        public bool IsSimulation { get; set; } = false;                // 是否为模拟设备
        public int StatusInterval { get; init; } = 3;                  // 状态检查间隔，单位秒，0表示不检查
        /// <summary>
        /// 采样频率
        /// </summary>
        public int SampleRate { get; init; }
        public List<HardwareSignalConfiguration> Signals { get; init; } = []; 
    }
}
