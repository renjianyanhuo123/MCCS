using MCCS.Collecter.HardwareDevices;

namespace MCCS.Collecter.SignalInterfaceManager
{
    /// <summary>
    /// 信号接口管理器 - 管理所有物理信号接口，实现数据采集隔离
    /// </summary>
    public interface ISignalManager : IDisposable
    {
        /// <summary>
        /// 初始化信号管理器，关联硬件设备
        /// </summary>
        /// <param name="device">硬件设备</param>
        void Initialize(IControllerHardwareDevice device);

        /// <summary>
        /// 添加物理信号接口
        /// </summary>
        /// <param name="signalConfig">信号配置</param>
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
        /// 启动所有信号采集
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
        /// 是否正在运行
        /// </summary>
        bool IsRunning { get; }
    }
}
