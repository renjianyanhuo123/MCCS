using MCCS.Collecter.HardwareDevices;

namespace MCCS.Collecter.ControllerManagers.Signals
{
    public sealed class HardwareSignalChannel : IDisposable
    {

        public HardwareSignalChannel(HardwareSignalConfiguration signalConfig)
        {
            Configuration = signalConfig;
            SignalAddressIndex = (long)signalConfig.SignalAddress;
            ConnectedDeviceId = signalConfig.DeviceId;
            SignalId = signalConfig.SignalId;
        }

        public long SignalId { get; private set; }

        public long? ConnectedDeviceId { get; private set; }

        public HardwareSignalConfiguration Configuration { get; }

        public long SignalAddressIndex { get; }

        public void Start()
        {
        }

        public void Stop()
        {
        }

        public void Dispose()
        {
            Stop();
        }
    }
}
