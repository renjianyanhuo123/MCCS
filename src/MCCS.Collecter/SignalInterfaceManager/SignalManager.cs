using MCCS.Collecter.HardwareDevices;
using System.Collections.Concurrent;

namespace MCCS.Collecter.SignalInterfaceManager
{
    /// <summary>
    /// 信号接口管理器 - 管理所有控制器及其物理信号接口，实现数据采集隔离
    /// </summary>
    public sealed class SignalManager : ISignalManager
    {
        private readonly ConcurrentDictionary<long, IControllerHardwareDevice> _devices;
        private readonly ConcurrentDictionary<long, HardwareSignalChannel> _physicalSignals;
        private bool _isInitialized;
        private readonly object _lockObject = new();

        public SignalManager()
        {
            _devices = new ConcurrentDictionary<long, IControllerHardwareDevice>();
            _physicalSignals = new ConcurrentDictionary<long, HardwareSignalChannel>();
            _isInitialized = false;
        }

        /// <summary>
        /// 是否正在运行（信号流已初始化）
        /// </summary>
        public bool IsRunning => _isInitialized;

        /// <summary>
        /// 添加硬件控制器设备
        /// </summary>
        public bool AddDevice(IControllerHardwareDevice device)
        {
            if (device == null)
                throw new ArgumentNullException(nameof(device));

            if (device is not ControllerHardwareDeviceBase deviceBase)
                throw new ArgumentException("设备类型不支持，必须继承自ControllerHardwareDeviceBase", nameof(device));

            var added = _devices.TryAdd(deviceBase.DeviceId, device);

            // 如果已经初始化，需要初始化该设备的所有信号流
            if (added && _isInitialized)
            {
                InitializeDeviceSignals(deviceBase.DeviceId);
            }

            return added;
        }

        /// <summary>
        /// 移除硬件控制器设备
        /// </summary>
        public bool RemoveDevice(long deviceId)
        {
            if (_devices.TryRemove(deviceId, out var device))
            {
                // 移除该设备的所有信号
                var signalsToRemove = _physicalSignals.Values
                    .Where(s => s.ConnectedDeviceId == deviceId)
                    .ToList();

                foreach (var signal in signalsToRemove)
                {
                    RemovePhysicalSignal(signal.SignalId);
                }

                return true;
            }
            return false;
        }

        /// <summary>
        /// 获取控制器设备
        /// </summary>
        public IControllerHardwareDevice? GetDevice(long deviceId)
        {
            _devices.TryGetValue(deviceId, out var device);
            return device;
        }

        /// <summary>
        /// 获取所有控制器设备
        /// </summary>
        public IReadOnlyCollection<IControllerHardwareDevice> GetAllDevices()
        {
            return _devices.Values.ToList().AsReadOnly();
        }

        /// <summary>
        /// 添加物理信号接口
        /// </summary>
        public bool AddPhysicalSignal(HardwareSignalConfiguration signalConfig)
        {
            if (signalConfig == null)
                throw new ArgumentNullException(nameof(signalConfig));

            if (!signalConfig.DeviceId.HasValue)
                throw new ArgumentException("信号配置必须指定DeviceId", nameof(signalConfig));

            // 检查信号是否已存在
            if (_physicalSignals.ContainsKey(signalConfig.SignalId))
                return false;

            // 检查设备是否存在
            if (!_devices.ContainsKey(signalConfig.DeviceId.Value))
                throw new InvalidOperationException($"设备 {signalConfig.DeviceId} 不存在，请先添加设备");

            var signal = new HardwareSignalChannel(signalConfig);
            var added = _physicalSignals.TryAdd(signalConfig.SignalId, signal);

            // 如果已经初始化，立即初始化新添加的信号流
            if (added && _isInitialized)
            {
                InitializeSignal(signal);
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
        /// 启动所有信号采集（初始化所有信号的数据流）
        /// </summary>
        public void Start()
        {
            lock (_lockObject)
            {
                if (_isInitialized)
                    return;

                // 初始化所有物理信号的数据流
                foreach (var signal in _physicalSignals.Values)
                {
                    InitializeSignal(signal);
                }

                _isInitialized = true;
            }
        }

        /// <summary>
        /// 停止所有信号采集
        /// 注意：由于使用 RefCount，流会在没有订阅者时自动停止，此方法主要用于标记状态
        /// </summary>
        public void Stop()
        {
            lock (_lockObject)
            {
                if (!_isInitialized)
                    return;

                // 使用 RefCount 的流会自动管理订阅，不需要手动停止
                _isInitialized = false;
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
        /// 根据设备ID获取该设备的所有物理信号
        /// </summary>
        public IReadOnlyCollection<HardwareSignalChannel> GetPhysicalSignalsByDevice(long deviceId)
        {
            return _physicalSignals.Values
                .Where(s => s.ConnectedDeviceId == deviceId)
                .ToList()
                .AsReadOnly();
        }

        /// <summary>
        /// 获取信号数据流（直接从设备流派生，不提前拆开）
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
        /// 检查设备是否存在
        /// </summary>
        public bool ContainsDevice(long deviceId)
        {
            return _devices.ContainsKey(deviceId);
        }

        /// <summary>
        /// 初始化单个信号流
        /// </summary>
        private void InitializeSignal(HardwareSignalChannel signal)
        {
            if (!signal.ConnectedDeviceId.HasValue)
                return;

            if (_devices.TryGetValue(signal.ConnectedDeviceId.Value, out var device))
            {
                if (device is ControllerHardwareDeviceBase deviceBase)
                {
                    signal.Initialize(deviceBase.IndividualDataStream);
                }
            }
        }

        /// <summary>
        /// 初始化指定设备的所有信号流
        /// </summary>
        private void InitializeDeviceSignals(long deviceId)
        {
            var signals = _physicalSignals.Values
                .Where(s => s.ConnectedDeviceId == deviceId);

            foreach (var signal in signals)
            {
                InitializeSignal(signal);
            }
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

            // 清理设备引用
            _devices.Clear();
        }
    }
}
