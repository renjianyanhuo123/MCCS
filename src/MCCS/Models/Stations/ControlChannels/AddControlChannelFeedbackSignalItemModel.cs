namespace MCCS.Models.Stations.ControlChannels
{
    public class AddControlChannelFeedbackSignalItemModel : BindableBase
    {
        private string _tempId;
        public string TempId
        {
            get => _tempId;
            set => SetProperty(ref _tempId, value);
        }
        // SignalTypeEnum
        private int _signalType;
        public int SignalType
        {
            get => _signalType;
            set => SetProperty(ref _signalType, value);
        }

        private ControlChannelSelectableItemModel _selectedSignalModel;
        public ControlChannelSelectableItemModel SelectedSignalModel
        {
            get => _selectedSignalModel;
            set
            {
                if (value.IsSelected) return;
                if (_selectedSignalModel != null) _selectedSignalModel.IsSelected = false;
                value.IsSelected = true;
                SetProperty(ref _selectedSignalModel, value);
            }
        }
    }

    public class EditControlChannelFeedbackSignalItemModel : BindableBase
    {
        private long _id;
        public long Id
        {
            get => _id;
            set => SetProperty(ref _id, value);
        }
        private string _tempId;
        public string TempId
        {
            get => _tempId;
            set => SetProperty(ref _tempId, value);
        }
        // SignalTypeEnum
        private int _signalType;
        public int SignalType
        {
            get => _signalType;
            set => SetProperty(ref _signalType, value);
        }

        private ControlChannelSelectableItemModel _selectedSignalModel;
        public ControlChannelSelectableItemModel SelectedSignalModel
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
