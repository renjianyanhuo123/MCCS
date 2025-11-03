using MCCS.Collecter.HardwareDevices;

namespace MCCS.Collecter.SignalInterfaceManager
{
    /// <summary>
    /// 所有的信号接口管理; 实现底层设备和控制器的隔离
    /// </summary>
    public interface ISignalManager : IDisposable
    {
        /// <summary>
        /// 初始化信号管理器，关联硬件设备
        /// </summary>
        void Initialize(IControllerHardwareDevice device);

        /// <summary>
        /// 启动所有信号采集和控制通道
        /// </summary>
        void Start();

        /// <summary>
        /// 停止所有信号采集和控制通道
        /// </summary>
        void Stop();

        /// <summary>
        /// 添加虚拟通道
        /// </summary>
        bool AddVirtualChannel(long channelId, string channelName, string formula, List<long> signalIds, double rangeMin = 0, double rangeMax = 100);

        /// <summary>
        /// 移除虚拟通道
        /// </summary>
        bool RemoveVirtualChannel(long channelId);

        /// <summary>
        /// 添加控制通道
        /// </summary>
        bool AddControlChannel(
            long channelId,
            string channelName,
            Core.Models.StationSites.ChannelTypeEnum channelType,
            Core.Models.StationSites.ControlChannelModeTypeEnum controlMode,
            double controlCycle,
            long? positionSignalId,
            long? forceSignalId,
            long? outputSignalId,
            short outputLimitation = 100);

        /// <summary>
        /// 移除控制通道
        /// </summary>
        bool RemoveControlChannel(long channelId);

        /// <summary>
        /// 获取物理信号
        /// </summary>
        HardwareSignalChannel? GetPhysicalSignal(long signalId);

        /// <summary>
        /// 获取虚拟通道
        /// </summary>
        VirtualChannel? GetVirtualChannel(long channelId);

        /// <summary>
        /// 获取控制通道
        /// </summary>
        ControlChannel? GetControlChannel(long channelId);

        /// <summary>
        /// 订阅信号数据流（可以是物理信号或虚拟通道）
        /// </summary>
        IObservable<SignalData>? GetSignalDataStream(long signalId);
    }
}
