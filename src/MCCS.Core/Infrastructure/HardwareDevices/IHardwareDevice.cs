namespace MCCS.Core.Infrastructure.HardwareDevices
{
    public interface IHardwareDevice
    {
        long DeviceId { get; }
        string DeviceName { get; }
        /// <summary>
        /// 设备类型
        /// </summary>
        string DeviceType { get; }                                     
        HardwareConnectionStatus Status { get; }

        /// <summary>
        /// 获取设备支持的所有接口
        /// </summary>
        /// <returns></returns>
        List<HardwareSignal> GetSupportedSignals();
        /// <summary>
        /// 获取特定信号接口信息
        /// </summary>
        /// <param name="signalId"></param>
        /// <returns></returns>
        HardwareSignal GetSignal(string signalId);
        /// <summary>
        /// 检查信号接口是否可用
        /// </summary>
        /// <param name="signalId"></param>
        /// <returns></returns>
        bool IsSignalAvailable(string signalId);                    

        // 设备连接方法
        Task<bool> ConnectAsync();
        Task<bool> DisconnectAsync();
        Task<bool> TestConnectionAsync();

        // 通道操作方法
        Task<double> ReadSignalAsync(string signalId);              // 读取通道值
        Task<bool> WriteSignalAsync(string signalId, double value); // 写入通道值
        /// <summary>
        /// 配置通道
        /// </summary>
        /// <param name="signalId"></param>
        /// <param name="config"></param>
        /// <returns></returns>
        Task<bool> ConfigureSignalAsync(string signalId, HardwareSignal config);

        /// <summary>
        /// 校准方法
        /// </summary>
        /// <param name="signalId"></param>
        /// <param name="referenceValue"></param>
        /// <returns></returns>
        Task<bool> CalibrateChannelAsync(string signalId, double referenceValue);
        /// <summary>
        /// 归零操作
        /// </summary>
        /// <param name="signalId"></param>
        /// <returns></returns>
        Task<bool> ZeroChannelAsync(string signalId);

        /// <summary>
        /// 事件
        /// </summary>
        event EventHandler<string> DeviceStatusChanged;
        event EventHandler<(string signalId, double value)> ChannelDataReceived;
        event EventHandler<(string signalId, string error)> ChannelError;
    }
}
