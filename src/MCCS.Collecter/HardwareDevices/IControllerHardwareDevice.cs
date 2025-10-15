namespace MCCS.Collecter.HardwareDevices
{
    public interface IControllerHardwareDevice : IDisposable
    {
        bool ConnectToHardware();
        bool DisconnectFromHardware();
        IObservable<DataPoint> GetSignalStream(string signalId);
        void StartDataAcquisition();
        void StopDataAcquisition(); 
    }
}
