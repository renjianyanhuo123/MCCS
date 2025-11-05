using MCCS.Collecter.ControllerManagers;
using MCCS.Collecter.HardwareDevices;

namespace MCCS.Collecter.SignalManagers
{
    public sealed class SignalManager : ISignalManager
    {
        private readonly IControllerManager _controllerManager;

        public SignalManager(IControllerManager controllerManager)
        {
            _controllerManager = controllerManager;
        }

        public IObservable<DataPoint<float>> GetSignalDataStream(long signalId)
        {
            var signChannel = Signals.FirstOrDefault(s => s.SignalId == signalId);
            if (signChannel == null) throw new ArgumentNullException("can't find signalInfo");
            var targetIndex = signChannel.SignalAddressIndex;
            return IndividualDataStream.Select(info => new DataPoint<float>
                {
                    DataQuality = info.DataQuality,
                    DeviceId = info.DeviceId,
                    Timestamp = info.Timestamp,
                    Value = (float)info.Value.CollectData.GetValueOrDefault(signChannel.SignalId, 0.0f)
                }).Publish()
                .RefCount();
        }

    }
}
