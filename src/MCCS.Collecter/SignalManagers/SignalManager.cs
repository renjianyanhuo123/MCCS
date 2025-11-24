using System.Reactive.Linq;
using MCCS.Collecter.ControllerManagers;
using MCCS.Collecter.HardwareDevices;
using MCCS.Collecter.SignalManagers.Signals;
using MCCS.Infrastructure.Models.ProjectManager;

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
                })
                .Publish()
                .RefCount();
        }

        public IObservable<List<ProjectDataRecordModel>> GetProjectDataRecords()
        {
            // TODO：只能取第一个控制器了，后面肯定要根据实际情况修改的
            var controller = _controllerManager.GetControllers().First();
            return controller.DataStream.Select(s =>
            {
                var res = new List<ProjectDataRecordModel>();
                foreach (var data in s.Value)
                {
                    var recordData = new ProjectDataRecordModel(); 
                    // 一次性采集所有的信号
                    foreach (var signal in _signals)
                    {
                        recordData.SignalItems.Add(new ProjectSignalItemModel
                        {
                            RecordId = recordData.RecordId,
                            SignalKey = "",
                            SignalId = signal.SignalId,
                            Unit = signal.Configuration.Unit,
                            Value = signal.SignalAddressIndex < 10 ? data.Net_AD_N[signal.SignalAddressIndex] : data.Net_AD_S[signal.SignalAddressIndex % 10]
                        });
                    }
                    res.Add(recordData);
                } 
                return res;
            });
        }

        #endregion 

    }
}
