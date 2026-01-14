using MCCS.Infrastructure.Models.StationSites;

namespace MCCS.Interface.Components.Models
{
    public class ControlChannelBindModel : BindableBase
    {
        private long _channelId;
        public long ChannelId 
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

        private ChannelTypeEnum _channelType; 
        public ChannelTypeEnum ChannelType
        {
            get => _channelType;
            set => SetProperty(ref _channelType, value);
        }
    }
}
