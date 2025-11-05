using MCCS.Collecter.DllNative.Models;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using MCCS.Infrastructure.TestModels;
using MCCS.Collecter.HardwareDevices;
using MCCS.Collecter.ControllerManagers.Signals;
using Newtonsoft.Json.Linq;

namespace MCCS.Collecter.ControllerManagers.Entities
{
    public abstract class ControllerHardwareDeviceBase : IControllerHardwareDevice
    { 
        protected readonly BehaviorSubject<HardwareConnectionStatus> _statusSubject;
        protected readonly ReplaySubject<DataPoint<List<BatchCollectItemModel>>> _dataSubject;
        protected readonly int _sampleRate;
        protected IDisposable? _statusSubscription; 
         
        // 是否正在采集数据
        protected bool _isRunning = false;

        /// <summary>
        /// 当前设备句柄
        /// </summary>
        protected nint _deviceHandle = nint.Zero;

        /// <summary>
        /// 设备ID
        /// </summary>
        public long DeviceId { get; }
        public string DeviceName { get; }
        public string DeviceType { get; }
        public HardwareConnectionStatus Status { get; protected set; }
        public List<HardwareSignalChannel> Signals { get; protected set; }

        /// <summary>
        /// 控制器当前所处的控制状态 
        /// </summary>
        public SystemControlState ControlState { get; protected set; }
        /// <summary>
        /// 批量数据流
        /// </summary>
        public IObservable<DataPoint<List<BatchCollectItemModel>>> DataStream { get; protected set; }
        /// <summary>
        /// 状态流
        /// </summary>
        public IObservable<HardwareConnectionStatus> StatusStream => _statusSubject.AsObservable();
        /// <summary>
        /// 不预先展开，而是提供展开后的Observable;
        /// 单个的数据流
        /// </summary>
        public IObservable<DataPoint<BatchCollectItemModel>> IndividualDataStream { get; protected set; }

        protected ControllerHardwareDeviceBase(HardwareDeviceConfiguration configuration)
        {
            DeviceId = configuration.DeviceId;
            DeviceName = configuration.DeviceName;
            DeviceType = configuration.DeviceType;
            _sampleRate = configuration.SampleRate;
            _statusSubject = new BehaviorSubject<HardwareConnectionStatus>(HardwareConnectionStatus.Disconnected);
            _dataSubject = new ReplaySubject<DataPoint<List<BatchCollectItemModel>>>(bufferSize: 1000);
            Signals = configuration.Signals.Select(s => new HardwareSignalChannel(s)).ToList();
            DataStream = _dataSubject.AsObservable();
            IndividualDataStream = _dataSubject.Where(dp => dp is { DataQuality: DataQuality.Good, Value: not null })
                .SelectMany(dataPoint =>
                    dataPoint.Value.Select(item =>
                        new DataPoint<BatchCollectItemModel>
                        {
                            Value = item,
                            Timestamp = dataPoint.Timestamp,
                            DeviceId = dataPoint.DeviceId
                        }
                    ));
        }

        // 抽象方法 
        public abstract bool ConnectToHardware();
        public abstract bool DisconnectFromHardware();
        /// <summary>
        /// 操作试验实验状态
        /// </summary>
        /// <param name="isStart">是否开始 0-停止  1-开始</param>
        public abstract bool OperationTest(uint isStart);
        /// <summary>
        /// 操作阀门
        /// </summary>
        /// <param name="isOpen"></param>
        /// <returns></returns>
        public abstract bool OperationValveState(bool isOpen);
        /// <summary>
        /// 切换控制方式
        /// </summary>
        /// <param name="controlState"></param>
        /// <returns></returns>
        public abstract bool OperationControlMode(SystemControlState controlState);
        //public DeviceCommandContext GetDeviceCommandContext(long deviceId)
        //{
        //    return _deviceContexts.GetValueOrDefault(deviceId, new DeviceCommandContext
        //    {
        //        DeviceId = deviceId,
        //        IsValid = false,
        //        CurrentStatus = CommandExecuteStatusEnum.NoExecute
        //    });
        //}  

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
            foreach (var signal in Signals)
            {
                var t = signal.SignalAddressIndex;
                if (t < 10 && t < model.Net_AD_N.Length)
                {
                    res.CollectData.Add(signal.SignalId, model.Net_AD_N[t]);
                    continue;
                }
                t %= 10;
                if (t < model.Net_AD_S.Length)
                    res.CollectData.Add(signal.SignalId, model.Net_AD_S[t]);
            }
            return res;
        }

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

        public virtual void Dispose()
        { 
            _statusSubscription?.Dispose();
            _statusSubject.OnCompleted();
            _statusSubject.Dispose(); 
        }
    }
}
