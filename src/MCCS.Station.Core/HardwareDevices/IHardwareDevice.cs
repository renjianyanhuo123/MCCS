namespace MCCS.Station.Core.HardwareDevices
{
    public interface IHardwareDevice : IDisposable
    {
        bool ConnectToHardware();
        bool DisconnectFromHardware(); 
        void StartDataAcquisition();
        void StopDataAcquisition(); 
    }
}
