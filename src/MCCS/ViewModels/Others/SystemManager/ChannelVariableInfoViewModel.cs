namespace MCCS.ViewModels.Others.SystemManager
{

    public class VariableInfoViewModel : BindableBase
    {
        private long _variableId;
        private string _variableName = string.Empty;

        public long VariableId
        {
            get => _variableId;
            set => SetProperty(ref _variableId, value);
        }

        public string VariableName
        {
            get => _variableName;
            set => SetProperty(ref _variableName, value);
        }
    }

    public sealed class ChannelVariableInfoViewModel : BindableBase
    {
        private long _channelId ;
        private string _channelName = string.Empty;
        private List<VariableInfoViewModel> _variableInfoViewModels = [];
        public long ChannelId
        {
            get => _channelId;
            set => SetProperty(ref _channelId, value);
        }
        public string ChannelName
        {
            get => _channelName;
            set => SetProperty(ref _channelName, value);
        }

        public List<VariableInfoViewModel> VariableInfos
        {
            get => _variableInfoViewModels;
            set => SetProperty(ref _variableInfoViewModels, value);
        }
    }
}
