using System.Reactive.Linq;

using MCCS.Station.Core.HardwareDevices;
using MCCS.Station.Core.SignalManagers;

namespace MCCS.Station.Core.PseudoChannelManagers
{
    /// <summary>
    /// 虚拟通道 - 组合多个信号源为单一数据流
    /// 支持多订阅者场景，使用 Publish().RefCount() 实现热Observable
    /// </summary>
    public sealed class PseudoChannel
    {
        private readonly ISignalManager _signalManager;
        private readonly Lazy<IObservable<DataPoint<float>>> _cachedStream;

        public PseudoChannel(PseudoChannelConfiguration configuration, ISignalManager signalManager)
        {
            Configuration = configuration;
            _signalManager = signalManager;
            ChannelId = configuration.ChannelId;

            // 延迟初始化流，确保多次调用返回相同的热Observable
            _cachedStream = new Lazy<IObservable<DataPoint<float>>>(CreatePseudoChannelStream);
        }

        public long ChannelId { get; init; }

        public PseudoChannelConfiguration Configuration { get; }

        /// <summary>
        /// 获取虚拟通道数据流
        /// 使用 CombineLatest 同步组合多个信号源
        /// </summary>
        public IObservable<DataPoint<float>> GetPseudoChannelStream() => _cachedStream.Value;

        private IObservable<DataPoint<float>> CreatePseudoChannelStream()
        {
            var signalStreamList = Configuration.SignalIds
                .Select(s => _signalManager.GetSignalDataStream(s))
                .ToList();

            if (signalStreamList.Count == 0)
                return Observable.Empty<DataPoint<float>>();

            // 单信号直接返回，避免不必要的CombineLatest开销
            // dp => dp with { Unit = Configuration.Unit ?? dp.Unit } 这种写法用于覆盖其他同名的属性
            if (signalStreamList.Count == 1)
            {
                return signalStreamList[0]
                    .Select(dp => dp with { Unit = Configuration.Unit ?? dp.Unit })
                    .Publish()
                    .RefCount();
            }

            // 多信号使用 CombineLatest 同步组合
            return signalStreamList
                .CombineLatest()
                .Select(values => new DataPoint<float>
                {
                    DeviceId = values[0].DeviceId, 
                    Timestamp = values.Max(s => s.Timestamp), // 使用最新时间戳而非平均值
                    Unit = Configuration.Unit ?? "",
                    Value = values.Average(s => s.Value)
                })
                .Publish()
                .RefCount();
        }
    }
}
