namespace MCCS.Collecter.HardwareDevices
{
    public interface IControllerHardwareDevice
    {
        Task<bool> ConnectToHardwareAsync();
        Task<bool> DisconnectFromHardwareAsync();
        IObservable<DataPoint> GetSignalStream(string signalId);
        void StartDataAcquisition();
        void StopDataAcquisition();
    }
}
