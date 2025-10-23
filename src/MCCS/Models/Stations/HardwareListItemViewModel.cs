using System.Collections.ObjectModel;
using MCCS.Core.Models.Devices;

namespace MCCS.Models.Stations
{

    public sealed class HardwareChildItemViewModel : BindableBase
    {
        public long HardwareId { get; set; }
        public string HardwareName { get; set; } = string.Empty;

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

        private bool _isSelectable;
        public bool IsSelectable
        {
            get => _isSelectable;
            set => SetProperty(ref _isSelectable, value);
        }

        private bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected;
            set => SetProperty(ref _isSelected, value);
        } 
    }

    public sealed class HardwareListItemViewModel : BindableBase
    {
        public long ControllerId { get; set; }
        public string ControllerName { get; set; } = string.Empty;

        public ObservableCollection<HardwareChildItemViewModel> ChildItems { get; private set; } = [];
    }

    public sealed class ControlChannelHardwareChildItemViewModel: BindableBase
    {
        public long HardwareId { get; set; }
        public string HardwareName { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
    }

    public sealed class ControlChannelHardwareListItemViewModel : BindableBase
    {
        public long ControllerId { get; set; }
        public string ControllerName { get; set; } = string.Empty;
        public ObservableCollection<ControlChannelHardwareChildItemViewModel> ChildItems { get; private set; } = [];
    }
}
