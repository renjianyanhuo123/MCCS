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
        private bool _isInitialized;
        private readonly object _lockObject = new();

        public SignalManager()
        {
            _physicalSignals = new ConcurrentDictionary<long, HardwareSignalChannel>();
            _isInitialized = false;
        }

        /// <summary>
        /// 是否正在运行（信号流已初始化）
        /// </summary>
        public bool IsRunning => _isInitialized;

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

            // 如果已经启动，立即初始化新添加的信号流
            if (added && _isInitialized && _device is ControllerHardwareDeviceBase deviceBase)
            {
                signal.Initialize(deviceBase.IndividualDataStream);
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

                if (_device == null)
                    throw new InvalidOperationException("设备未初始化，请先调用Initialize方法");

                if (_device is not ControllerHardwareDeviceBase deviceBase)
                    throw new InvalidOperationException("设备类型不支持，必须继承自ControllerHardwareDeviceBase");

                // 初始化所有物理信号的数据流（直接从设备流派生，不使用 Subject 转发）
                foreach (var signal in _physicalSignals.Values)
                {
                    signal.Initialize(deviceBase.IndividualDataStream);
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
                // 这里只是标记状态
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
