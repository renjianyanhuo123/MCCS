namespace MCCS.ViewModels.Others.SystemManager
{
    public class ChannelHardwareViewModel : BindableBase
    {
        private long _hardwareId;
        public long HardwareId
        {
            get => _hardwareId;
            set => SetProperty(ref _hardwareId, value);
        }

        private string _hardwareName = string.Empty;

        public string HardwareName
        {
            get => _hardwareName;
            set => SetProperty(ref _hardwareName, value);
        }

        private string _controllerName = string.Empty;
        public string ControllerName
        {
            get => _controllerName;
            set => SetProperty(ref _controllerName, value);
        }

    }
}
