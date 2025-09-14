namespace MCCS.Collecter.HardwareDevices.BwController
{
    public class BwControllerHardwareDevice : ControllerHardwareDeviceBase
    {
        public BwControllerHardwareDevice(HardwareDeviceConfiguration configuration) : base(configuration)
        {
        }

        protected override Task<bool> ConnectToHardwareAsync()
        {
            throw new NotImplementedException();
        }

        protected override Task<bool> DisconnectFromHardwareAsync()
        {
            throw new NotImplementedException();
        }
    }
}
