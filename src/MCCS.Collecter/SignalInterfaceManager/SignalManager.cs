using MCCS.Collecter.HardwareDevices;
using MCCS.Core.Models.StationSites;
using System.Collections.Concurrent;

namespace MCCS.Collecter.SignalInterfaceManager
{
    /// <summary>
    /// 信号管理器 - 统一管理物理信号、虚拟通道和控制通道
    /// 实现数据采集和控制的隔离
    /// </summary>
    public sealed class SignalManager : ISignalManager
    {
        private readonly ConcurrentDictionary<long, HardwareSignalChannel> _physicalSignals;
        private readonly ConcurrentDictionary<long, VirtualChannel> _virtualChannels;
        private readonly ConcurrentDictionary<long, ControlChannel> _controlChannels;

        private IControllerHardwareDevice? _device;
        private bool _isRunning;

        public SignalManager()
        {
            _physicalSignals = new ConcurrentDictionary<long, HardwareSignalChannel>();
            _virtualChannels = new ConcurrentDictionary<long, VirtualChannel>();
            _controlChannels = new ConcurrentDictionary<long, ControlChannel>();
        }

        /// <summary>
        /// 初始化信号管理器，关联硬件设备
        /// </summary>
        public void Initialize(IControllerHardwareDevice device)
        {
            if (_device != null)
                throw new InvalidOperationException("SignalManager已经初始化过");

            _device = device;

            // 从设备的基类中获取已配置的物理信号
            if (device is ControllerHardwareDeviceBase deviceBase)
            {
                // 通过反射或其他方式获取 _signals（这里需要设备基类提供访问方式）
                // 暂时假设设备提供了获取信号的方法
                // 由于ControllerHardwareDeviceBase的_signals是protected，我们需要另外的方式
                // 这里简化处理，假设设备会在配置中提供信号列表
            }
        }

        /// <summary>
        /// 从配置初始化物理信号
        /// </summary>
        public void InitializePhysicalSignals(List<HardwareSignalConfiguration> signalConfigs)
        {
            foreach (var config in signalConfigs)
            {
                var signal = new HardwareSignalChannel(config);
                _physicalSignals.TryAdd(config.SignalId, signal);
            }
        }

        /// <summary>
        /// 启动所有信号采集和控制通道
        /// </summary>
        public void Start()
        {
            if (_isRunning)
                return;

            if (_device == null)
                throw new InvalidOperationException("设备未初始化，请先调用Initialize");

            // 启动所有物理信号采集
            if (_device is ControllerHardwareDeviceBase deviceBase)
            {
                foreach (var signal in _physicalSignals.Values)
                {
                    signal.Start(deviceBase.IndividualDataStream);
                }
            }

            // 启动所有虚拟通道
            foreach (var channel in _virtualChannels.Values)
            {
                channel.Start(_physicalSignals.ToDictionary(kvp => kvp.Key, kvp => kvp.Value));
            }

            // 启动所有控制通道
            foreach (var channel in _controlChannels.Values)
            {
                channel.Start(
                    _physicalSignals.ToDictionary(kvp => kvp.Key, kvp => kvp.Value),
                    _virtualChannels.ToDictionary(kvp => kvp.Key, kvp => kvp.Value),
                    _device);
            }

            _isRunning = true;
        }

        /// <summary>
        /// 停止所有信号采集和控制通道
        /// </summary>
        public void Stop()
        {
            if (!_isRunning)
                return;

            // 停止所有控制通道
            foreach (var channel in _controlChannels.Values)
            {
                channel.Stop();
            }

            // 停止所有虚拟通道
            foreach (var channel in _virtualChannels.Values)
            {
                channel.Stop();
            }

            // 停止所有物理信号
            foreach (var signal in _physicalSignals.Values)
            {
                signal.Stop();
            }

            _isRunning = false;
        }

        /// <summary>
        /// 添加虚拟通道
        /// </summary>
        public bool AddVirtualChannel(
            long channelId,
            string channelName,
            string formula,
            List<long> signalIds,
            double rangeMin = 0,
            double rangeMax = 100)
        {
            if (_virtualChannels.ContainsKey(channelId))
                return false;

            var channel = new VirtualChannel(
                channelId,
                channelName,
                formula,
                signalIds,
                rangeMin,
                rangeMax);

            var added = _virtualChannels.TryAdd(channelId, channel);

            // 如果已经在运行，立即启动新通道
            if (added && _isRunning)
            {
                channel.Start(_physicalSignals.ToDictionary(kvp => kvp.Key, kvp => kvp.Value));
            }

            return added;
        }

        /// <summary>
        /// 移除虚拟通道
        /// </summary>
        public bool RemoveVirtualChannel(long channelId)
        {
            if (_virtualChannels.TryRemove(channelId, out var channel))
            {
                channel.Dispose();
                return true;
            }
            return false;
        }

        /// <summary>
        /// 添加控制通道
        /// </summary>
        public bool AddControlChannel(
            long channelId,
            string channelName,
            ChannelTypeEnum channelType,
            ControlChannelModeTypeEnum controlMode,
            double controlCycle,
            long? positionSignalId,
            long? forceSignalId,
            long? outputSignalId,
            short outputLimitation = 100)
        {
            if (_controlChannels.ContainsKey(channelId))
                return false;

            var channel = new ControlChannel(
                channelId,
                channelName,
                channelType,
                controlMode,
                controlCycle,
                outputLimitation);

            channel.BindFeedbackSignals(positionSignalId, forceSignalId, outputSignalId);

            var added = _controlChannels.TryAdd(channelId, channel);

            // 如果已经在运行，立即启动新通道
            if (added && _isRunning && _device != null)
            {
                channel.Start(
                    _physicalSignals.ToDictionary(kvp => kvp.Key, kvp => kvp.Value),
                    _virtualChannels.ToDictionary(kvp => kvp.Key, kvp => kvp.Value),
                    _device);
            }

            return added;
        }

        /// <summary>
        /// 移除控制通道
        /// </summary>
        public bool RemoveControlChannel(long channelId)
        {
            if (_controlChannels.TryRemove(channelId, out var channel))
            {
                channel.Dispose();
                return true;
            }
            return false;
        }

        /// <summary>
        /// 获取物理信号
        /// </summary>
        public HardwareSignalChannel? GetPhysicalSignal(long signalId)
        {
            _physicalSignals.TryGetValue(signalId, out var signal);
            return signal;
        }

        /// <summary>
        /// 获取虚拟通道
        /// </summary>
        public VirtualChannel? GetVirtualChannel(long channelId)
        {
            _virtualChannels.TryGetValue(channelId, out var channel);
            return channel;
        }

        /// <summary>
        /// 获取控制通道
        /// </summary>
        public ControlChannel? GetControlChannel(long channelId)
        {
            _controlChannels.TryGetValue(channelId, out var channel);
            return channel;
        }

        /// <summary>
        /// 订阅信号数据流（可以是物理信号或虚拟通道）
        /// </summary>
        public IObservable<SignalData>? GetSignalDataStream(long signalId)
        {
            // 先从物理信号查找
            if (_physicalSignals.TryGetValue(signalId, out var physicalSignal))
            {
                return physicalSignal.DataStream;
            }

            // 再从虚拟通道查找
            if (_virtualChannels.TryGetValue(signalId, out var virtualChannel))
            {
                return virtualChannel.DataStream;
            }

            return null;
        }

        public void Dispose()
        {
            Stop();

            // 释放所有控制通道
            foreach (var channel in _controlChannels.Values)
            {
                channel.Dispose();
            }
            _controlChannels.Clear();

            // 释放所有虚拟通道
            foreach (var channel in _virtualChannels.Values)
            {
                channel.Dispose();
            }
            _virtualChannels.Clear();

            // 释放所有物理信号
            foreach (var signal in _physicalSignals.Values)
            {
                signal.Dispose();
            }
            _physicalSignals.Clear();
        }
    }
}
