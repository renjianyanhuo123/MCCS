namespace MCCS.Collecter.HardwareDevices
{
    public sealed class HardwareSignalChannel : IDisposable
    {
        private readonly HardwareSignalConfiguration _configuration;

        public HardwareSignalChannel(HardwareSignalConfiguration signalConfig)
        {
            _configuration = signalConfig;
            SignalAddressIndex = (long)signalConfig.SignalAddress;
            SignalId = signalConfig.SignalId;
        }

        public long SignalId { get; private set; } 

        public long SignalAddressIndex { get; private set; }
         
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
