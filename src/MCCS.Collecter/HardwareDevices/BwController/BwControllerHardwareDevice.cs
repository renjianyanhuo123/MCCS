namespace MCCS.Collecter.HardwareDevices.BwController
{
    public sealed class BwControllerHardwareDevice : ControllerHardwareDeviceBase
    {
        public BwControllerHardwareDevice(HardwareDeviceConfiguration configuration) : base(configuration)
        {
        }

        public override Task<bool> ConnectToHardwareAsync()
        {
            throw new NotImplementedException();
        }

        public override Task<bool> DisconnectFromHardwareAsync()
        {
            throw new NotImplementedException();
        }
    }
}
