using System.Reactive.Linq;
using System.Reactive.Subjects;

using MCCS.Infrastructure.TestModels;
using MCCS.Station.Core.DllNative.Models;
using MCCS.Station.Core.HardwareDevices;

namespace MCCS.Station.Core.ControllerManagers.Entities
{
    public abstract class ControllerHardwareDeviceBase
    { 
        // ReSharper disable once InconsistentNaming
        protected readonly BehaviorSubject<HardwareConnectionStatus> _statusSubject;
        // ReSharper disable once InconsistentNaming
        protected readonly int _sampleRate;
        // ReSharper disable once InconsistentNaming
        protected IDisposable? _statusSubscription; 
         
        // 是否正在采集数据
        // ReSharper disable once InconsistentNaming
        protected volatile bool _isRunning = false;

        /// <summary>
        /// 当前设备句柄
        /// </summary>
        // ReSharper disable once InconsistentNaming
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
        public IObservable<SampleBatch<TNet_ADHInfo>> DataStream { get; protected set; }

        /// <summary>
        /// 状态流
        /// </summary>
        public IObservable<HardwareConnectionStatus> StatusStream { get; protected set; }

        /// <summary>
        /// 单个的数据流 - 从批量数据展开为单条数据
        /// 使用 Publish().RefCount() 支持多订阅者场景
        /// </summary>
        public IObservable<DataPoint<TNet_ADHInfo>> IndividualDataStream { get; protected set; }

        protected ControllerHardwareDeviceBase(HardwareDeviceConfiguration configuration)
        {
            DeviceId = configuration.DeviceId;
            DeviceName = configuration.DeviceName;
            DeviceType = configuration.DeviceType; 
            _sampleRate = configuration.SampleRate;
            _statusSubject = new BehaviorSubject<HardwareConnectionStatus>(HardwareConnectionStatus.Disconnected); 
            StatusStream = _statusSubject.AsObservable(); 
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
