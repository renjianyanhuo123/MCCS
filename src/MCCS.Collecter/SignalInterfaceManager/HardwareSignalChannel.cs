using MCCS.Collecter.HardwareDevices;
using MCCS.Collecter.DllNative.Models;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace MCCS.Collecter.SignalInterfaceManager
{
    /// <summary>
    /// 物理信号通道 - 从硬件设备数据流中提取对应物理信号的数据
    /// </summary>
    public sealed class HardwareSignalChannel : IDisposable
    {
        private readonly Subject<SignalData> _dataSubject;
        private IDisposable? _dataSubscription;
        private bool _isRunning;

        public HardwareSignalChannel(HardwareSignalConfiguration signalConfig)
        {
            Configuration = signalConfig;
            ConnectedDeviceId = signalConfig.DeviceId;
            SignalId = signalConfig.SignalId;
            _dataSubject = new Subject<SignalData>();
            DataStream = _dataSubject.AsObservable();
        }

        public long SignalId { get; private set; }

        public long? ConnectedDeviceId { get; private set; }

        public HardwareSignalConfiguration Configuration { get; }

        /// <summary>
        /// 信号数据流
        /// </summary>
        public IObservable<SignalData> DataStream { get; }

        public long SignalAddressIndex
        {
            get
            {
                var index = (long)Configuration.SignalAddress;
                if (index < 10)
                {
                    return index;
                }
                else
                {
                    return index % 10;
                }
            }
        }

        /// <summary>
        /// 启动信号采集，订阅硬件设备的数据流
        /// </summary>
        /// <param name="deviceDataStream">硬件设备的数据流</param>
        public void Start(IObservable<BatchCollectItemModel> deviceDataStream)
        {
            if (_isRunning) return;

            _dataSubscription = deviceDataStream
                .Where(data => data != null)
                .Select(data => ExtractSignalData(data))
                .Where(signalData => signalData != null)
                .Subscribe(
                    signalData => _dataSubject.OnNext(signalData!),
                    error => { /* 错误处理暂时忽略 */ }
                );

            _isRunning = true;
        }

        /// <summary>
        /// 停止信号采集
        /// </summary>
        public void Stop()
        {
            if (!_isRunning) return;

            _dataSubscription?.Dispose();
            _dataSubscription = null;
            _isRunning = false;
        }

        /// <summary>
        /// 从批量采集数据中提取对应的信号数据
        /// </summary>
        private SignalData? ExtractSignalData(BatchCollectItemModel batchData)
        {
            // 尝试从 Net_AD_N 中获取数据
            if (batchData.Net_AD_N.TryGetValue(SignalId, out var value))
            {
                return new SignalData
                {
                    SignalId = SignalId,
                    Value = value,
                    Timestamp = batchData.Net_TimeCnt,
                    IsValid = true
                };
            }

            // 尝试从 Net_AD_S 中获取数据
            if (batchData.Net_AD_S.TryGetValue(SignalId, out value))
            {
                return new SignalData
                {
                    SignalId = SignalId,
                    Value = value,
                    Timestamp = batchData.Net_TimeCnt,
                    IsValid = true
                };
            }

            return null;
        }

        public void Dispose()
        {
            Stop();
            _dataSubject.OnCompleted();
            _dataSubject.Dispose();
        }
    }
}
