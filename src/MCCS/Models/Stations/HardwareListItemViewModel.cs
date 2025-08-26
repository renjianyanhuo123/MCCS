using System.Collections.ObjectModel;
using MCCS.Core.Models.Devices;

namespace MCCS.Models.Stations
{

    public sealed class HardwareChildItemViewModel : BindableBase
    {
        public long HardwareId { get; set; }
        public string HardwareName { get; set; } = string.Empty;
        private bool _isSelectable;
        public bool IsSelectable
        {
            get => _isSelectable;
            set => SetProperty(ref _isSelectable, value);
        }

        private DeviceStatusEnum _deviceStatus = DeviceStatusEnum.Unknown;
        public DeviceStatusEnum DeviceStatus
        {
            get => _deviceStatus;
            set => SetProperty(ref _deviceStatus, value);
        }
    }

    public sealed class HardwareListItemViewModel : BindableBase
    {
        public long ControllerId { get; set; }
        public string ControllerName { get; set; } = string.Empty;

        public ObservableCollection<HardwareChildItemViewModel> ChildItems { get; private set; } = [];
    }
}
