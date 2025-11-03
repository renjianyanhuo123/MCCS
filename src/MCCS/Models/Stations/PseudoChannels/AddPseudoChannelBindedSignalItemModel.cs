namespace MCCS.Models.Stations.PseudoChannels
{
    public class AddPseudoChannelBindedSignalItemModel : BindableBase
    {
        private string _tempId = string.Empty;
        public string TempId
        {
            get => _tempId;
            set => SetProperty(ref _tempId, value);
        }

        private PseudoChannelSelectableItemModel? _selectedSignalModel;
        public PseudoChannelSelectableItemModel? SelectedSignalModel
        {
            get => _selectedSignalModel;
            set
            {
                if (value != null && value.IsSelected) return;
                if (_selectedSignalModel != null) _selectedSignalModel.IsSelected = false;
                if (value != null) value.IsSelected = true;
                SetProperty(ref _selectedSignalModel, value);
            }
        }
    }

    public class EditPseudoChannelBindedSignalItemModel : BindableBase
    {
        private long _id;
        public long Id
        {
            get => _id;
            set => SetProperty(ref _id, value);
        }

        private string _tempId = string.Empty;
        public string TempId
        {
            get => _tempId;
            set => SetProperty(ref _tempId, value);
        }

        private PseudoChannelSelectableItemModel? _selectedSignalModel;
        public PseudoChannelSelectableItemModel? SelectedSignalModel
        {
            get => _selectedSignalModel;
            set
            {
                if (value == null || value.IsSelected) return;
                if (_selectedSignalModel != null) _selectedSignalModel.IsSelected = false;
                value.IsSelected = true;
                SetProperty(ref _selectedSignalModel, value);
            }
        }
    }
}
