using System.Reactive.Linq;
using MCCS.Collecter.ControllerManagers;
using MCCS.Collecter.HardwareDevices;
using MCCS.Collecter.SignalManagers.Signals;

namespace MCCS.Collecter.SignalManagers
{
    public sealed class SignalManager : ISignalManager
    {
        private readonly IControllerManager _controllerManager;
        private readonly List<HardwareSignalChannel> _signals = [];

        public SignalManager(IControllerManager controllerManager)
        {
            _controllerManager = controllerManager;
        } 

        public void Initialization(IEnumerable<HardwareSignalConfiguration> signalConfigurations)
        {
            foreach (var configuration in signalConfigurations)
            {
                _signals.Add(new HardwareSignalChannel(configuration));
            }
        } 

        #region 采集信号
        public IObservable<DataPoint<float>> GetSignalDataStream(long signalId)
        {
            var signChannel = _signals.FirstOrDefault(s => s.SignalId == signalId);
            if (signChannel == null) throw new ArgumentNullException("can't find signalInfo");
            var controller = _controllerManager.GetControllerInfo(signChannel.BelongControllerId);
            return controller.IndividualDataStream.Select(info =>
                {
                    var tempModel = new DataPoint<float>
                    {
                        DataQuality = info.DataQuality,
                        DeviceId = info.DeviceId,
                        Timestamp = info.Timestamp,
                        Value = signChannel.SignalAddressIndex < 10 ? info.Value.Net_AD_N[signChannel.SignalAddressIndex] : info.Value.Net_AD_S[signChannel.SignalAddressIndex % 10]
                    };
                    return tempModel;
                }).Publish()
                .RefCount();
        } 

        #endregion

    }
}
