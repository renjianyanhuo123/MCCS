using MCCS.Collecter.DllNative.Models;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using MCCS.Infrastructure.TestModels;
using MCCS.Collecter.HardwareDevices;

namespace MCCS.Collecter.ControllerManagers.Entities
{
    public abstract class ControllerHardwareDeviceBase
    { 
        protected readonly BehaviorSubject<HardwareConnectionStatus> _statusSubject;
        protected readonly ReplaySubject<DataPoint<List<TNet_ADHInfo>>> _dataSubject;
        protected readonly int _sampleRate;
        protected IDisposable? _statusSubscription; 
         
        // 是否正在采集数据
        protected volatile bool _isRunning = false;

        /// <summary>
        /// 当前设备句柄
        /// </summary>
        protected nint _deviceHandle = nint.Zero;
         
        public long DeviceId { get; } 
        public string DeviceName { get; }
        public string DeviceType { get; }

        public HardwareConnectionStatus Status { get; protected set; } 

        /// <summary>
        /// 控制器当前所处的控制状态 
        /// </summary>
        public SystemControlState ControlState { get; protected set; }
        /// <summary>
        /// 批量数据流
        /// </summary>
        public IObservable<DataPoint<List<TNet_ADHInfo>>> DataStream { get; protected set; }

        /// <summary>
        /// 状态流
        /// </summary>
        public IObservable<HardwareConnectionStatus> StatusStream { get; protected set; }

        /// <summary>
        /// 不预先展开，而是提供展开后的Observable;
        /// 单个的数据流
        /// </summary>
        public IObservable<DataPoint<TNet_ADHInfo>> IndividualDataStream { get; protected set; }

        protected ControllerHardwareDeviceBase(HardwareDeviceConfiguration configuration)
        {
            DeviceId = configuration.DeviceId;
            DeviceName = configuration.DeviceName;
            DeviceType = configuration.DeviceType; 
            _sampleRate = configuration.SampleRate;
            _statusSubject = new BehaviorSubject<HardwareConnectionStatus>(HardwareConnectionStatus.Disconnected);
            _dataSubject = new ReplaySubject<DataPoint<List<TNet_ADHInfo>>>(bufferSize: 1000);
            StatusStream = _statusSubject.AsObservable();
            DataStream = _dataSubject.AsObservable();
            IndividualDataStream = _dataSubject.Where(dp => dp is { DataQuality: DataQuality.Good, Value: not null })
                .SelectMany(dataPoint =>
                    dataPoint.Value.Select(item =>
                        new DataPoint<TNet_ADHInfo>
                        {
                            Value = item,
                            Unit = "",
                            Timestamp = dataPoint.Timestamp,
                            DeviceId = dataPoint.DeviceId
                        }
                    ));
        }
         
        #region 抽象方法 
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

        public virtual SystemControlState GetControlState()
        {
            return ControlState;
        } 
        #endregion

        public virtual void Dispose()
        {
            _statusSubscription?.Dispose();
            _statusSubject.OnCompleted();
            _statusSubject.Dispose();
        } 
    }
}
