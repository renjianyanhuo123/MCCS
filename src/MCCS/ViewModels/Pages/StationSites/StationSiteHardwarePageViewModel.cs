using System.Collections.ObjectModel;
using MCCS.Core.Models.Devices;
using MCCS.Core.Repositories;
using MCCS.Models.Stations;

namespace MCCS.ViewModels.Pages.StationSites
{
    public sealed class StationSiteHardwarePageViewModel : BaseViewModel
    {
        public const string Tag = "StationSiteHardware";

        private readonly IDeviceInfoRepository _deviceInfoRepository;
        private readonly IStationSiteRepository _stationSiteRepository;

        public StationSiteHardwarePageViewModel(IEventAggregator eventAggregator,
            IDeviceInfoRepository deviceInfoRepository,
            IStationSiteRepository stationSiteRepository) : base(eventAggregator)
        {
            _deviceInfoRepository = deviceInfoRepository;
            _stationSiteRepository = stationSiteRepository;
        }

        #region Property
        public ObservableCollection<StationSiteHardwareItemModel> StationSiteHardwareItems { get; private set; } = [];

        public ObservableCollection<HardwareListItemViewModel> HardwareListItems { get; private set; } = [];
        #endregion

        #region Command
        public AsyncDelegateCommand LoadCommand => new(ExecuteLoadCommand);
        #endregion

        #region Private Method
        private async Task ExecuteLoadCommand()
        {
            StationSiteHardwareItems.Clear();
            HardwareListItems.Clear();
            // var devices = await _deviceInfoRepository.GetAllDevicesAsync();
            // 右侧所有的设备
            var allDevices = await _deviceInfoRepository.GetAllDevicesAsync();
            var parentIds = allDevices
                .Where(s => s.MainDeviceId != null)
                .Select(s => s.MainDeviceId)
                .Distinct()
                .ToList();
            var parentInfos = allDevices
                .Where(c => parentIds.Contains(c.DeviceId))
                .ToList();
            foreach (var parentInfo in parentInfos)
            {
                var hardwareListItem = new HardwareListItemViewModel
                {
                    ControllerId = parentInfo.Id,
                    ControllerName = parentInfo.DeviceName,
                };
                var childItems = allDevices
                    .Where(c => c.MainDeviceId == parentInfo.DeviceId)
                    .Select(c =>
                    {
                        var isSelected = selectedHardwareIds.Any(h => h == c.Id);
                        return new HardwareChildItemViewModel
                        {
                            HardwareId = c.Id,
                            HardwareName = c.DeviceName,
                            IsSelectable = !isSelected,
                            DeviceStatus = DeviceStatusEnum.Connected,
                        };
                    }).ToList();
                foreach (var item in childItems)
                {
                    hardwareListItem.ChildItems.Add(item);
                }
                HardwareListItems.Add(hardwareListItem);
            }
        }
        #endregion
    }
}
