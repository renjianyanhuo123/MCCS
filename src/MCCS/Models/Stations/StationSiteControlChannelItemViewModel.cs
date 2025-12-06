using System.Collections.ObjectModel;
using MCCS.Infrastructure.Models.StationSites;

namespace MCCS.Models.Stations
{
    public class StationSiteControlChannelSignalViewModel: BindableBase
    {
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
        private SignalTypeEnum _signalType;
        public SignalTypeEnum SignalType
        {
            get => _signalType; 
            set => SetProperty(ref _signalType, value);
        }

        private string _controllerName = string.Empty;
        public string ControllerName
        {
            get => _controllerName;
            set => SetProperty(ref _controllerName, value);
        }

        private string _deviceName = string.Empty;
        public string DeviceName
        {
            get => _deviceName;
            set => SetProperty(ref _deviceName, value);
        }
    }

    public class StationSiteControlChannelItemViewModel : BindableBase
    {
        private long _id;
        public long Id
        {
            get => _id;
            set => SetProperty(ref _id, value);
        }

        private string _channelId = string.Empty;
        public string ChannelId
        {
            get => _channelId; 
            set => SetProperty(ref _channelId, value);
        }

        private string _channelName = string.Empty;
        public string ChannelName
        {
            get => _channelName; 
            set => SetProperty(ref _channelName, value);
        }
        private ControlChannelModeTypeEnum _controlMode;
        public ControlChannelModeTypeEnum ControlMode
        {
            get => _controlMode;
            set => SetProperty(ref _controlMode, value);
        }
        public ObservableCollection<StationSiteControlChannelSignalViewModel> Signals { get; set; } = [];
    }
}
