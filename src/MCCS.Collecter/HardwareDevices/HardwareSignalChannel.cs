namespace MCCS.Collecter.HardwareDevices
{
    public sealed class HardwareSignalChannel : IDisposable
    {
        private readonly HardwareSignalConfiguration _configuration;

        public HardwareSignalChannel(HardwareSignalConfiguration signalConfig)
        {
            _configuration = signalConfig;
            DeviceId = signalConfig.DeviceId;
            SignalId = signalConfig.SignalId;
        }

        public long SignalId { get; private set; }

        public long? DeviceId { get; private set; }

        public long SignalAddressIndex {
            get
            {
                var index = (long)_configuration.SignalAddress;
                if (index < 10)
                {
                    return index;
                }
                else
                {
                    return index % 10;
                } 
            }
        }

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
