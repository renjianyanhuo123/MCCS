using System.Reactive.Linq;

using MCCS.Station.HardwareDevices;
using MCCS.Station.SignalManagers;

namespace MCCS.Station.PseudoChannelManagers
{
    public sealed class PseudoChannel
    {  
        private readonly ISignalManager _signalManager;

        public PseudoChannel(PseudoChannelConfiguration configuration, ISignalManager signalManager)
        {
            Configuration = configuration;
            _signalManager = signalManager;
            ChannelId = configuration.ChannelId;
        }

        public long ChannelId { get; init; }

        public PseudoChannelConfiguration Configuration { get; }

        public IObservable<DataPoint<float>> GetPseudoChannelStream()
        {
            var signalStreamList = Configuration.SignalConfigurations.Select(s => _signalManager.GetSignalDataStream(s.SignalId)).ToList();
            return signalStreamList.CombineLatest().Select(values =>
                new DataPoint<float>()
                {
                    DeviceId = values[0].DeviceId,
                    DataQuality = values.All(s => s.DataQuality == DataQuality.Good) ? DataQuality.Good : DataQuality.Bad,
                    Timestamp = (long)values.Average(s => s.Timestamp),
                    Unit = Configuration.Unit ?? "",
                    Value = values.Average(s => s.Value)
                });
        }

    }
}
