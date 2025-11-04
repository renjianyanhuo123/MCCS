using MCCS.Collecter.HardwareDevices;
using System.Collections.Concurrent;

namespace MCCS.Collecter.SignalInterfaceManager
{
    /// <summary>
    /// 信号接口管理器 - 管理所有物理信号接口，实现数据采集隔离
    /// </summary>
    public sealed class SignalManager : ISignalManager
    {
        private readonly ConcurrentDictionary<long, HardwareSignalChannel> _physicalSignals;
        private IControllerHardwareDevice? _device;
        private bool _isRunning;
        private readonly object _lockObject = new();

        public SignalManager()
        {
            _physicalSignals = new ConcurrentDictionary<long, HardwareSignalChannel>();
            _isRunning = false;
        }

        /// <summary>
        /// 是否正在运行
        /// </summary>
        public bool IsRunning => _isRunning;

        /// <summary>
        /// 初始化信号管理器，关联硬件设备
        /// </summary>
        public void Initialize(IControllerHardwareDevice device)
        {
            lock (_lockObject)
            {
                if (_device != null)
                    throw new InvalidOperationException("SignalManager已经初始化过，不能重复初始化");

                _device = device ?? throw new ArgumentNullException(nameof(device));
            }
        }

        /// <summary>
        /// 添加物理信号接口
        /// </summary>
        public bool AddPhysicalSignal(HardwareSignalConfiguration signalConfig)
        {
            if (signalConfig == null)
                throw new ArgumentNullException(nameof(signalConfig));

            // 检查信号是否已存在
            if (_physicalSignals.ContainsKey(signalConfig.SignalId))
                return false;

            var signal = new HardwareSignalChannel(signalConfig);
            var added = _physicalSignals.TryAdd(signalConfig.SignalId, signal);

            // 如果已经在运行，立即启动新添加的信号
            if (added && _isRunning && _device is ControllerHardwareDeviceBase deviceBase)
            {
                signal.Start(deviceBase.IndividualDataStream);
            }

            return added;
        }

        /// <summary>
        /// 批量添加物理信号接口
        /// </summary>
        public void AddPhysicalSignals(IEnumerable<HardwareSignalConfiguration> signalConfigs)
        {
            if (signalConfigs == null)
                throw new ArgumentNullException(nameof(signalConfigs));

            foreach (var config in signalConfigs)
            {
                AddPhysicalSignal(config);
            }
        }

        /// <summary>
        /// 移除物理信号接口
        /// </summary>
        public bool RemovePhysicalSignal(long signalId)
        {
            if (_physicalSignals.TryRemove(signalId, out var signal))
            {
                signal.Dispose();
                return true;
            }
            return false;
        }

        /// <summary>
        /// 启动所有信号采集
        /// </summary>
        public void Start()
        {
            lock (_lockObject)
            {
                if (_isRunning)
                    return;

                if (_device == null)
                    throw new InvalidOperationException("设备未初始化，请先调用Initialize方法");

                if (_device is not ControllerHardwareDeviceBase deviceBase)
                    throw new InvalidOperationException("设备类型不支持，必须继承自ControllerHardwareDeviceBase");

                // 启动所有物理信号采集
                foreach (var signal in _physicalSignals.Values)
                {
                    signal.Start(deviceBase.IndividualDataStream);
                }

                _isRunning = true;
            }
        }

        /// <summary>
        /// 停止所有信号采集
        /// </summary>
        public void Stop()
        {
            lock (_lockObject)
            {
                if (!_isRunning)
                    return;

                // 停止所有物理信号
                foreach (var signal in _physicalSignals.Values)
                {
                    signal.Stop();
                }

                _isRunning = false;
            }
        }

        /// <summary>
        /// 获取物理信号接口
        /// </summary>
        public HardwareSignalChannel? GetPhysicalSignal(long signalId)
        {
            _physicalSignals.TryGetValue(signalId, out var signal);
            return signal;
        }

        /// <summary>
        /// 获取所有物理信号接口
        /// </summary>
        public IReadOnlyCollection<HardwareSignalChannel> GetAllPhysicalSignals()
        {
            return _physicalSignals.Values.ToList().AsReadOnly();
        }

        /// <summary>
        /// 获取信号数据流
        /// </summary>
        public IObservable<SignalData>? GetSignalDataStream(long signalId)
        {
            if (_physicalSignals.TryGetValue(signalId, out var signal))
            {
                return signal.DataStream;
            }
            return null;
        }

        /// <summary>
        /// 检查信号是否存在
        /// </summary>
        public bool ContainsSignal(long signalId)
        {
            return _physicalSignals.ContainsKey(signalId);
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose()
        {
            Stop();

            // 释放所有物理信号
            foreach (var signal in _physicalSignals.Values)
            {
                signal.Dispose();
            }
            _physicalSignals.Clear();

            _device = null;
        }
    }
}
