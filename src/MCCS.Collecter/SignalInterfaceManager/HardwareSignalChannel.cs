using MCCS.Collecter.HardwareDevices;
using MCCS.Collecter.DllNative.Models;
using System.Reactive.Linq;

namespace MCCS.Collecter.SignalInterfaceManager
{
    /// <summary>
    /// 物理信号通道 - 从硬件设备数据流中提取对应物理信号的数据
    /// 使用 Rx 操作符直接派生数据流，不使用 Subject 转发
    /// </summary>
    public sealed class HardwareSignalChannel : IDisposable
    {
        private IObservable<SignalData>? _dataStream;

        public HardwareSignalChannel(HardwareSignalConfiguration signalConfig)
        {
            Configuration = signalConfig;
            ConnectedDeviceId = signalConfig.DeviceId;
            SignalId = signalConfig.SignalId;
        }

        public long SignalId { get; }

        public long? ConnectedDeviceId { get; }

        public HardwareSignalConfiguration Configuration { get; }

        /// <summary>
        /// 信号数据流 - 直接从设备流派生，不提前拆开
        /// </summary>
        public IObservable<SignalData> DataStream => _dataStream
            ?? throw new InvalidOperationException($"信号 {SignalId} 未初始化，请先调用 Initialize 方法");

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
        /// 初始化信号数据流，直接从设备流派生
        /// </summary>
        /// <param name="deviceDataStream">硬件设备的数据流</param>
        public void Initialize(IObservable<BatchCollectItemModel> deviceDataStream)
        {
            if (_dataStream != null)
                throw new InvalidOperationException($"信号 {SignalId} 已经初始化过");

            // 直接使用 Rx 操作符从源流派生，不使用 Subject 转发
            _dataStream = deviceDataStream
                .Select(data => ExtractSignalData(data))
                .Where(signalData => signalData != null)
                .Select(signalData => signalData!)
                .Publish()      // 转为热流
                .RefCount();    // 自动管理订阅，有订阅者时连接，无订阅者时断开
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
            // RefCount 会自动管理订阅的生命周期，不需要手动清理
            _dataStream = null;
        }
    }
}
