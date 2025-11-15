namespace MCCS.Models.Model3D
{
    public class MapDeviceModel : BindableBase
    {
        private long _deviceId;
        public long DeviceId 
        { 
            get => _deviceId;
            set => SetProperty(ref _deviceId, value);
        }

        private string _deviceName; 
        public string DeviceName
        {
            get => _deviceName;
            set => SetProperty(ref _deviceName, value);
        }
    }
}
