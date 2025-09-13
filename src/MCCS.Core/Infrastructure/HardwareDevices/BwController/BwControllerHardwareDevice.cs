namespace MCCS.Core.Infrastructure.HardwareDevices.BwController
{
    public class BwControllerHardwareDevice : IHardwareDevice
    {
        public long DeviceId { get; }
        public string DeviceName { get; }
        public string DeviceType { get; }
        public HardwareConnectionStatus Status { get; }
        public List<HardwareSignal> GetSupportedSignals()
        {
            throw new NotImplementedException();
        }

        public HardwareSignal GetSignal(string signalId)
        {
            throw new NotImplementedException();
        }

        public bool IsSignalAvailable(string signalId)
        {
            throw new NotImplementedException();
        }

        public Task<bool> ConnectAsync()
        {
            throw new NotImplementedException();
        }

        public Task<bool> DisconnectAsync()
        {
            throw new NotImplementedException();
        }

        public Task<bool> TestConnectionAsync()
        {
            throw new NotImplementedException();
        }

        public Task<double> ReadSignalAsync(string signalId)
        {
            throw new NotImplementedException();
        }

        public Task<bool> WriteSignalAsync(string signalId, double value)
        {
            throw new NotImplementedException();
        }

        public Task<bool> ConfigureSignalAsync(string signalId, HardwareSignal config)
        {
            throw new NotImplementedException();
        }

        public Task<bool> CalibrateChannelAsync(string signalId, double referenceValue)
        {
            throw new NotImplementedException();
        }

        public Task<bool> ZeroChannelAsync(string signalId)
        {
            throw new NotImplementedException();
        }

        public event EventHandler<string>? DeviceStatusChanged;
        public event EventHandler<(string signalId, double value)>? ChannelDataReceived;
        public event EventHandler<(string signalId, string error)>? ChannelError;
    }
}
