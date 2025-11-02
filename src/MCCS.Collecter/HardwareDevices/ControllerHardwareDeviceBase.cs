using MCCS.Collecter.DllNative.Models;
using System.Collections.Concurrent;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using MCCS.Infrastructure.TestModels;
using MCCS.Infrastructure.TestModels.ControlParams;
using MCCS.Infrastructure.TestModels.Commands;

namespace MCCS.Collecter.HardwareDevices
{
    public abstract class ControllerHardwareDeviceBase :  IControllerHardwareDevice
    {
        protected readonly ConcurrentDictionary<long, HardwareSignalChannel> _signals = new();
        protected readonly BehaviorSubject<HardwareConnectionStatus> _statusSubject;
        protected readonly ReplaySubject<DataPoint> _dataSubject;
        protected IDisposable? _statusSubscription;

        protected readonly Subject<CommandStatusChangeEvent> _commandStatusSubject;
        protected readonly ConcurrentDictionary<long, DeviceCommandContext> _deviceContexts = new();
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

        /// <summary>
        /// 命令状态变化流（包含设备ID信息）
        /// </summary>
        public IObservable<CommandStatusChangeEvent> CommandStatusStream => _commandStatusSubject.AsObservable();
        /// <summary>
        /// 控制器当前所处的控制状态 
        /// </summary>
        public SystemControlState ControlState { get; protected set; }
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
            _commandStatusSubject = new Subject<CommandStatusChangeEvent>();
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

        public DeviceCommandContext GetDeviceCommandContext(long deviceId)
        {
            return _deviceContexts.GetValueOrDefault(deviceId, new DeviceCommandContext
            {
                DeviceId = deviceId,
                CurrentStatus = CommandExecuteStatusEnum.NoExecute
            });
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


        #region 可重写的方法
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
        /// <summary>
        /// 手动控制
        /// </summary>
        /// <param name="deviceId">对应连接的作动器设备ID</param>
        /// <param name="outValue">位移运动速度</param>
        /// <returns></returns>
        public abstract bool ManualControl(long deviceId, float outValue);
        /// <summary>
        /// 静态控制
        /// </summary>
        /// <param name="controlParam">静态控制参数</param>
        /// <returns></returns>
        public abstract bool StaticControl(StaticControlParams controlParam);
        /// <summary>
        /// 疲劳控制
        /// </summary>
        /// <param name="controlParam">动态控制参数</param>
        /// <returns></returns>
        public abstract bool DynamicControl(DynamicControlParams controlParam);
        #endregion

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
            _commandStatusSubject?.OnCompleted();
            _commandStatusSubject?.Dispose();
            _deviceContexts.Clear();
        }
    }
}
