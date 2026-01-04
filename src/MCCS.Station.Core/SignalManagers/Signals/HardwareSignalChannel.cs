namespace MCCS.Station.Core.SignalManagers.Signals
{
    public sealed class HardwareSignalChannel : IDisposable
    { 
        public HardwareSignalChannel(HardwareSignalConfiguration signalConfig)
        {
            Configuration = signalConfig;
            SignalAddressIndex = (long)signalConfig.SignalAddress;
            ConnectedDeviceId = signalConfig.DeviceId;
            SignalId = signalConfig.SignalId;
            BelongControllerId = signalConfig.BelongControllerId;
        }

        public long SignalId { get; private set; }

        public long? ConnectedDeviceId { get; private set; } 
        /// <summary>
        /// 所属的控制器ID
        /// </summary>
        public long BelongControllerId { get; set; }

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
