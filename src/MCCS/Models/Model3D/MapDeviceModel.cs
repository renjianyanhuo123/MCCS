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

        private string _deviceName = string.Empty;
        public string DeviceName
        {
            get => _deviceName;
            set => SetProperty(ref _deviceName, value);
        }

        private bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected;
            set => SetProperty(ref _isSelected, value);
        }
    }
}
