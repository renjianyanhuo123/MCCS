using MCCS.Collecter.DllNative.Models;
using System.Collections.Concurrent;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace MCCS.Collecter.HardwareDevices
{
    public abstract class ControllerHardwareDeviceBase :  IControllerHardwareDevice
    {
        protected readonly ConcurrentDictionary<long, HardwareSignalChannel> _signals = new();
        protected readonly BehaviorSubject<HardwareConnectionStatus> _statusSubject;
        protected readonly ReplaySubject<DataPoint> _dataSubject;
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
        public long DeviceId { get; }
        public string DeviceName { get; }
        public string DeviceType { get; }
        public HardwareConnectionStatus Status { get; protected set; }

        public IObservable<DataPoint> DataStream { get; protected set; }
        public IObservable<HardwareConnectionStatus> StatusStream => _statusSubject.AsObservable();
        // 不预先展开，而是提供展开后的Observable;单个的数据流
        public IObservable<BatchCollectItemModel> IndividualDataStream { get; protected set; }

        protected ControllerHardwareDeviceBase(HardwareDeviceConfiguration configuration)
        {
            DeviceId = configuration.DeviceId;
            DeviceName = configuration.DeviceName;
            DeviceType = configuration.DeviceType;
            _statusSubject = new BehaviorSubject<HardwareConnectionStatus>(HardwareConnectionStatus.Disconnected);
            _dataSubject = new ReplaySubject<DataPoint>(bufferSize: 1000);
            foreach (var item in configuration.Signals)
            {
                AddSignal(item);
            }
            DataStream = _dataSubject.AsObservable();
            IndividualDataStream = _dataSubject.Where(dp => dp is { DataQuality: DataQuality.Good, Value: List<BatchCollectItemModel> })
                .SelectMany(dp => (List<BatchCollectItemModel>)dp.Value);
        }

        // 抽象方法 
        public abstract bool ConnectToHardware();
        public abstract bool DisconnectFromHardware();
         
        public HardwareSignalChannel GetSignal(long signalId) => _signals.GetValueOrDefault(signalId);
        public bool IsSignalAvailable(long signalId) => _signals.ContainsKey(signalId); 

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

        protected void RemoveSignal(long signalId)
        {
            var success = _signals.TryGetValue(signalId, out var signal);
            if(success) signal?.Dispose();
        }

        protected BatchCollectItemModel StructDataToCollectModel(TNet_ADHInfo model)
        {
            var res = new BatchCollectItemModel
            {
                Net_PosVref = model.Net_PosVref,
                Net_PosE = model.Net_PosE,
                Net_CtrlDA = model.Net_CtrlDA,
                Net_CycleCount = model.Net_CycleCount,
                Net_SysState = model.Net_SysState,
                Net_DIVal = model.Net_DIVal,
                Net_DOVal = model.Net_DOVal,
                Net_D_PosVref = model.Net_D_PosVref,
                Net_FeedLoadN = model.Net_FeedLoadN,
                Net_PrtErrState = model.Net_PrtErrState,
                Net_TimeCnt = model.Net_TimeCnt
            }; 
            foreach (var signal in _signals.Values)
            {
                var t = signal.SignalAddressIndex;
                if (t < 10 && t < model.Net_AD_N.Length)
                {
                    res.Net_AD_N.Add(signal.SignalId, model.Net_AD_N[t]);
                    continue;
                }
                t %= 10;
                if(t < model.Net_AD_S.Length)
                    res.Net_AD_S.Add(signal.SignalId, model.Net_AD_S[t]);
            }
            return res;
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
