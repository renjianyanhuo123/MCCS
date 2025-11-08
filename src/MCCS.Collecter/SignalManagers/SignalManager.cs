using System.Reactive.Linq;
using MCCS.Collecter.ControllerManagers;
using MCCS.Collecter.DllNative;
using MCCS.Collecter.HardwareDevices;
using MCCS.Collecter.SignalManagers.Signals;
using MCCS.Infrastructure.TestModels;
using MCCS.Infrastructure.TestModels.ControlParams;

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

        public SystemControlState GetControlStateBySignalId(long signalId)
        {
            // 目前还在控制器中, 后面要改进的
            var signalInfo = _signals.FirstOrDefault(s => s.SignalId == signalId);
            if (signalInfo == null) throw new ArgumentNullException("controller no find signal");
            var controller = _controllerManager.GetControllerInfo(signalInfo.BelongControllerId); 
            return controller.ControlState;
        }

        public bool SetControlStateBySignalId(long signalId, SystemControlState controlMode)
        {
            // 目前还在控制器中, 后面要改进的
            var signalInfo = _signals.FirstOrDefault(s => s.SignalId == signalId);
            if (signalInfo == null) throw new ArgumentNullException("controller no find signal"); 
            return _controllerManager.OperationControlMode(signalInfo.BelongControllerId, controlMode);
        }

        public int SetStaticControlMode(long signalId, StaticLoadControlEnum tmpCtrlMode, float tmpVelo, float tmpPos)
        {
            var signalInfo = _signals.FirstOrDefault(s => s.SignalId == signalId);
            if (signalInfo == null) throw new ArgumentNullException("controller no find signal");
            var controller = _controllerManager.GetControllerInfo(signalInfo.BelongControllerId);
            var deviceHandle = controller.GetDeviceHandle();
            return POPNetCtrl.NetCtrl01_S_SetCtrlMod(deviceHandle, (uint)tmpCtrlMode, tmpVelo, tmpPos);
        }

        public int SetValleyPeakFilterNum(long signalId, int freq)
        {
            var signalInfo = _signals.FirstOrDefault(s => s.SignalId == signalId);
            if (signalInfo == null) throw new ArgumentNullException("controller no find signal");
            var controller = _controllerManager.GetControllerInfo(signalInfo.BelongControllerId);
            var deviceHandle = controller.GetDeviceHandle();
            return POPNetCtrl.NetCtrl01_bWriteAddr(deviceHandle, AddressContanst.Addr_ValleyPeak_FilterNum, (byte)freq);
        }

        public int SetDynamicControlMode(long signalId, float tmpMeanA, float tmpA,
            float tmpFreq, byte tmpWaveShap, byte tmpCtrlMode, float tmpAP, float tmpPH,
            int tmpCountSet, int tmpCtrlOpt)
        {
            var signalInfo = _signals.FirstOrDefault(s => s.SignalId == signalId);
            if (signalInfo == null) throw new ArgumentNullException("controller no find signal");
            var controller = _controllerManager.GetControllerInfo(signalInfo.BelongControllerId); 
            return POPNetCtrl.NetCtrl01_Osci_SetWaveInfo(controller.DeviceHandleIndex, tmpMeanA, tmpA, tmpFreq, tmpWaveShap, tmpCtrlMode, tmpAP, tmpPH, tmpCountSet, tmpCtrlOpt);
        }

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

    }
}
