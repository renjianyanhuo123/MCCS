using System.Collections.Concurrent;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace MCCS.Collecter.HardwareDevices
{
    public abstract class ControllerHardwareDeviceBase : IControllerHardwareDevice
    {
        protected readonly ConcurrentDictionary<string, HardwareSignalChannel> _signals = new();
        protected readonly BehaviorSubject<HardwareConnectionStatus> _statusSubject;

        public long DeviceId { get; }
        public string DeviceName { get; }
        public string DeviceType { get; }
        public HardwareConnectionStatus Status => _statusSubject.Value;

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
        public abstract Task<bool> ConnectToHardwareAsync();
        public abstract Task<bool> DisconnectFromHardwareAsync();

        // public virtual List<HardwareSignalChannel> GetSupportedSignals() => [.._signals.Values];
        public HardwareSignalChannel GetSignal(string signalId) => _signals.GetValueOrDefault(signalId);
        public bool IsSignalAvailable(string signalId) => _signals.ContainsKey(signalId);

        /// <summary>
        /// 获取单个信号的独立数据流 - 高性能，无额外开销
        /// </summary>
        public IObservable<DataPoint> GetSignalStream(string signalId)
        {
            return _signals.TryGetValue(signalId, out var channel) ? channel.DataStream : Observable.Empty<DataPoint>();
        } 

        public void StartDataAcquisition()
        {
            if (Status != HardwareConnectionStatus.Connected)
            {
                throw new InvalidOperationException("设备未连接");
            } 
            foreach (var signal in _signals.Values)
            {
                signal.Start();
            } 
        }

        public void StopDataAcquisition()
        {
            foreach (var signal in _signals.Values)
            {
                signal.Stop();
            } 
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

            _statusSubject.OnCompleted();
            _statusSubject.Dispose();
        }
    }
}
