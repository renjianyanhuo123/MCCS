using MCCS.Collecter.HardwareDevices;
using System.Reactive.Linq;
using MCCS.Collecter.SignalManagers;

namespace MCCS.Collecter.PseudoChannelManagers
{
    public sealed class PseudoChannel
    { 
        private readonly PseudoChannelConfiguration _configuration;
        private readonly ISignalManager _signalManager;

        public PseudoChannel(PseudoChannelConfiguration configuration, ISignalManager signalManager)
        {
            _configuration = configuration;
            _signalManager = signalManager;
            ChannelId = configuration.ChannelId;
        }

        public long ChannelId { get; init; }

        public IObservable<DataPoint<float>> GetPseudoChannelStream()
        {
            var signalStreamList = _configuration.SignalConfigurations.Select(s => _signalManager.GetSignalDataStream(s.SignalId)).ToList();
            return signalStreamList.CombineLatest().Select(values =>
                new DataPoint<float>()
                {
                    DeviceId = values[0].DeviceId,
                    DataQuality = values.All(s => s.DataQuality == DataQuality.Good) ? DataQuality.Good : DataQuality.Bad,
                    Timestamp = (long)values.Average(s => s.Timestamp),
                    Unit = _configuration.Unit ?? "",
                    Value = values.Average(s => s.Value)
                });
        }

    }
}
