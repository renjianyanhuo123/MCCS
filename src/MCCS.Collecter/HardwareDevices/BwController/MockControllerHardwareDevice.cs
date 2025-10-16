using System.Reactive.Subjects;

namespace MCCS.Collecter.HardwareDevices.BwController
{
    public class MockControllerHardwareDevice : ControllerHardwareDeviceBase
    {
        private readonly ReplaySubject<DataPoint> _dataSubject;

        public MockControllerHardwareDevice(HardwareDeviceConfiguration configuration) : base(configuration)
        {
            _dataSubject = new ReplaySubject<DataPoint>(bufferSize: 1000);
        }

        public HardwareConnectionStatus Status { get; private set; }

        public IObservable<DataPoint> DataStream => _dataSubject.AsObservable();

        public override bool ConnectToHardware()
        {
            Status = HardwareConnectionStatus.Connected;
            return true;
        }

        public override bool DisconnectFromHardware()
        {
            Status = HardwareConnectionStatus.Disconnected;
            return true;
        }
    }
}
