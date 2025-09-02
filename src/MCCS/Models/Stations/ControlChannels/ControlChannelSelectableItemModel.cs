namespace MCCS.Models.Stations.ControlChannels
{
    public class ControlChannelSelectableItemModel : BindableBase
    {
        private long _deviceId;
        public long DeviceId
        {
            get => _deviceId; set => SetProperty(ref _deviceId, value);
        }

        private string _deviceName = string.Empty;
        public string DeviceName
        {
            get => _deviceName; set => SetProperty(ref _deviceName, value);
        }

        private string _controllerName = string.Empty;
        public string ControllerName
        {
            get => _controllerName; set => SetProperty(ref _controllerName, value);
        }

        private long _signalId;
        public long SignalId
        {
            get => _signalId; set => SetProperty(ref _signalId, value);
        }

        private string _signalName = string.Empty;
        public string SignalName
        {
            get => _signalName; set => SetProperty(ref _signalName, value);
        }

        private bool _isSelected = false;
        public bool IsSelected
        {
            get => _isSelected;
            set => SetProperty(ref _isSelected, value);
        }
    }
}
