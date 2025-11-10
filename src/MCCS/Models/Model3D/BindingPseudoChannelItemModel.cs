namespace MCCS.Models.Model3D
{
    public class BindingPseudoChannelItemModel : BindableBase
    {
        private long _id;
        public long Id
        {
            get => _id;
            set => SetProperty(ref _id, value);
        } 


        private string _pseudoChannelName = string.Empty;
        public string PseudoChannelName
        {
            get => _pseudoChannelName;
            set => SetProperty(ref _pseudoChannelName, value);
        }

        private bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected;
            set => SetProperty(ref _isSelected, value);
        }
    }
}
