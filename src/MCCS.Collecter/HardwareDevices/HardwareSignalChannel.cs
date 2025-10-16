namespace MCCS.Collecter.HardwareDevices
{
    public class HardwareSignalChannel : IDisposable
    {
        private readonly HardwareSignalConfiguration _signalConfig;  
        public string SignalId => _signalConfig.SignalId;
        public HardwareSignalConfiguration SignalConfig => _signalConfig; 

        public HardwareSignalChannel(HardwareSignalConfiguration signalConfig)
        {
            _signalConfig = signalConfig;  
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
