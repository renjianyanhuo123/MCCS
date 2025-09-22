namespace MCCS.Models.Model3D
{
    public class BindingControlChannelItemModel : BindableBase
    {
        private long _id;
        public long Id
        {
            get => _id;
            set => SetProperty(ref _id, value);
        }

        private string _key;
        public string Key
        {
            get => _key;
            set => SetProperty(ref _key, value);
        }

        private string _controlChannelName = string.Empty;
        public string ControlChannelName
        {
            get => _controlChannelName;
            set => SetProperty(ref _controlChannelName, value);
        }

        private bool _isSelected; 
        public bool IsSelected
        {
            get => _isSelected;
            set => SetProperty(ref _isSelected, value);
        }
    }
}
