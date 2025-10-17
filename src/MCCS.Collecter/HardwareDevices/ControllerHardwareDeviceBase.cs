using System.Collections.Concurrent;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace MCCS.Collecter.HardwareDevices
{
    public abstract class ControllerHardwareDeviceBase : IControllerHardwareDevice
    {
        protected readonly ConcurrentDictionary<string, HardwareSignalChannel> _signals = new();
        protected readonly BehaviorSubject<HardwareConnectionStatus> _statusSubject;
        protected IDisposable? _statusSubscription;
        // 是否正在采集数据
        protected bool _isRunning = false;

        /// <summary>
        /// 当前设备句柄
        /// </summary>
        protected IntPtr _deviceHandle = IntPtr.Zero;

        /// <summary>
        /// 设备ID
        /// </summary>
        public int DeviceId { get; }
        public string DeviceName { get; }
        public string DeviceType { get; }
        public HardwareConnectionStatus Status { get; protected set; }

        public IObservable<HardwareConnectionStatus> StatusStream => _statusSubject.AsObservable();

        protected ControllerHardwareDeviceBase(HardwareDeviceConfiguration configuration)
        {
            DeviceId = configuration.DeviceId;
            DeviceName = configuration.DeviceName;
            DeviceType = configuration.DeviceType;
            _statusSubject = new BehaviorSubject<HardwareConnectionStatus>(HardwareConnectionStatus.Disconnected);
            foreach (var item in configuration.Signals)
            {
                AddSignal(item);
            }
        }

        // 抽象方法 
        public abstract bool ConnectToHardware();
        public abstract bool DisconnectFromHardware();
         
        public HardwareSignalChannel GetSignal(string signalId) => _signals.GetValueOrDefault(signalId);
        public bool IsSignalAvailable(string signalId) => _signals.ContainsKey(signalId); 

        public virtual void StartDataAcquisition()
        {
            if (Status != HardwareConnectionStatus.Connected)
            {
                throw new InvalidOperationException("设备未连接");
            }
            _isRunning = true;
        }

        public virtual void StopDataAcquisition()
        {
            _isRunning = false;
        }

        protected void AddSignal(HardwareSignalConfiguration signalConfiguration)
        {
            var signalInfo = new HardwareSignalChannel(signalConfiguration);
            _signals.TryAdd(signalConfiguration.SignalId, signalInfo);
        }

        protected void RemoveSignal(string signalId)
        {
            var success = _signals.TryGetValue(signalId, out var signal);
            if(success) signal?.Dispose();
        }

        public virtual void Dispose()
        {
            foreach (var channel in _signals.Values)
            {
                channel.Dispose();
            }
            _statusSubscription?.Dispose();
            _statusSubject.OnCompleted();
            _statusSubject.Dispose();
        }
    }
}
