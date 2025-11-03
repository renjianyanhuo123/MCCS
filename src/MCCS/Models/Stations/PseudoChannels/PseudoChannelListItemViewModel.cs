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

        private string _name; 
        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        private string _createTime; 
        public string CreateTime
        {
            get => _createTime;
            set => SetProperty(ref _createTime, value);
        }

        private string _updateTime; 
        public string UpdateTime
        {
            get => _updateTime;
            set => SetProperty(ref _updateTime, value);
        }
    }
}
