namespace MCCS.Models.Stations
{
    public class StationSiteInfoViewModel:BindableBase
    {
        public long StationId { get; set; }
        public string StationName { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        private bool _isUsing = false;
        public bool IsUsing 
        { 
            get => _isUsing;
            set => SetProperty(ref _isUsing, value);
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
