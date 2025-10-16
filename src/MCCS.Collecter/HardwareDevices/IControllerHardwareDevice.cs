namespace MCCS.Collecter.HardwareDevices
{
    public interface IControllerHardwareDevice : IDisposable
    {
        bool ConnectToHardware();
        bool DisconnectFromHardware(); 
        void StartDataAcquisition();
        void StopDataAcquisition(); 
    }
}
