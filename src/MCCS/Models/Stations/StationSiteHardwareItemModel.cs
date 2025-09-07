namespace MCCS.Models.Stations
{
    public class StationSiteHardwareItemModel : BindableBase
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
        /// <summary>
        /// 控制器名称
        /// </summary>
        public string ControllerName
        {
            get => _controllerName;
            set => SetProperty(ref _controllerName, value);
        }

        private long _signalId; 
        public long SignalId
        {
            get => _signalId;
            set => SetProperty(ref _signalId, value);
        }

        private string _signalName = string.Empty;
        public string SignalName
        {
            get => _signalName;
            set => SetProperty(ref _signalName, value);
        }
    }
}
