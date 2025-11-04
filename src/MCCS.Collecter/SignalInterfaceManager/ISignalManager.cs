using MCCS.Collecter.HardwareDevices;

namespace MCCS.Collecter.SignalInterfaceManager
{
    /// <summary>
    /// 信号接口管理器 - 管理所有控制器及其物理信号接口，实现数据采集隔离
    /// </summary>
    public interface ISignalManager : IDisposable
    {
        /// <summary>
        /// 添加硬件控制器设备
        /// </summary>
        /// <param name="device">硬件控制器设备</param>
        /// <returns>是否添加成功</returns>
        bool AddDevice(IControllerHardwareDevice device);

        /// <summary>
        /// 移除硬件控制器设备
        /// </summary>
        /// <param name="deviceId">设备ID</param>
        /// <returns>是否移除成功</returns>
        bool RemoveDevice(long deviceId);

        /// <summary>
        /// 获取控制器设备
        /// </summary>
        /// <param name="deviceId">设备ID</param>
        /// <returns>控制器设备，如果不存在返回null</returns>
        IControllerHardwareDevice? GetDevice(long deviceId);

        /// <summary>
        /// 获取所有控制器设备
        /// </summary>
        /// <returns>控制器设备集合</returns>
        IReadOnlyCollection<IControllerHardwareDevice> GetAllDevices();

        /// <summary>
        /// 添加物理信号接口
        /// </summary>
        /// <param name="signalConfig">信号配置（需包含DeviceId关联到对应控制器）</param>
        /// <returns>是否添加成功</returns>
        bool AddPhysicalSignal(HardwareSignalConfiguration signalConfig);

        /// <summary>
        /// 批量添加物理信号接口
        /// </summary>
        /// <param name="signalConfigs">信号配置列表</param>
        void AddPhysicalSignals(IEnumerable<HardwareSignalConfiguration> signalConfigs);

        /// <summary>
        /// 移除物理信号接口
        /// </summary>
        /// <param name="signalId">信号ID</param>
        /// <returns>是否移除成功</returns>
        bool RemovePhysicalSignal(long signalId);

        /// <summary>
        /// 启动所有信号采集（初始化所有信号的数据流）
        /// </summary>
        void Start();

        /// <summary>
        /// 停止所有信号采集
        /// </summary>
        void Stop();

        /// <summary>
        /// 获取物理信号接口
        /// </summary>
        /// <param name="signalId">信号ID</param>
        /// <returns>物理信号接口，如果不存在返回null</returns>
        HardwareSignalChannel? GetPhysicalSignal(long signalId);

        /// <summary>
        /// 获取所有物理信号接口
        /// </summary>
        /// <returns>物理信号接口集合</returns>
        IReadOnlyCollection<HardwareSignalChannel> GetAllPhysicalSignals();

        /// <summary>
        /// 根据设备ID获取该设备的所有物理信号
        /// </summary>
        /// <param name="deviceId">设备ID</param>
        /// <returns>该设备的物理信号集合</returns>
        IReadOnlyCollection<HardwareSignalChannel> GetPhysicalSignalsByDevice(long deviceId);

        /// <summary>
        /// 获取信号数据流
        /// </summary>
        /// <param name="signalId">信号ID</param>
        /// <returns>信号数据流，如果不存在返回null</returns>
        IObservable<SignalData>? GetSignalDataStream(long signalId);

        /// <summary>
        /// 检查信号是否存在
        /// </summary>
        /// <param name="signalId">信号ID</param>
        /// <returns>是否存在</returns>
        bool ContainsSignal(long signalId);

        /// <summary>
        /// 检查设备是否存在
        /// </summary>
        /// <param name="deviceId">设备ID</param>
        /// <returns>是否存在</returns>
        bool ContainsDevice(long deviceId);

        /// <summary>
        /// 是否正在运行
        /// </summary>
        bool IsRunning { get; }
    }
}
