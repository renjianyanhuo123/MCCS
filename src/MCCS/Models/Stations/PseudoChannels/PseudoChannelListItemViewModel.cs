namespace MCCS.Models.Stations.PseudoChannels
{
    public sealed class PseudoChannelListItemViewModel : BindableBase
    {
        private long _id;
        public long Id
        {
            get => _id;
            set => SetProperty(ref _id, value);
        }

        private string _name = string.Empty;  
        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        private string _unit = string.Empty;
        public string Unit
        {
            get => _unit;
            set => SetProperty(ref _unit, value);
        }

        private string _createTime = string.Empty; 
        public string CreateTime
        {
            get => _createTime;
            set => SetProperty(ref _createTime, value);
        }

        private string _updateTime = string.Empty; 
        public string UpdateTime
        {
            get => _updateTime;
            set => SetProperty(ref _updateTime, value);
        }
    }
}
