using System.Collections.ObjectModel;
using MCCS.Core.Models.Devices;
using MCCS.Core.Models.StationSites;
using MCCS.Core.Repositories;
using MCCS.Models.Stations;
using MCCS.ViewModels.Others.SystemManager;

namespace MCCS.ViewModels.Pages.StationSites
{
    public sealed class StationSiteHardwarePageViewModel : BaseViewModel
    {
        public const string Tag = "StationSiteHardware";

        private readonly IDeviceInfoRepository _deviceInfoRepository;
        private readonly IStationSiteAggregateRepository _stationSiteRepository;
        private long _stationId = -1;

        public StationSiteHardwarePageViewModel(IEventAggregator eventAggregator,
            IDeviceInfoRepository deviceInfoRepository,
            IStationSiteAggregateRepository stationSiteRepository) : base(eventAggregator)
        {
            _deviceInfoRepository = deviceInfoRepository;
            _stationSiteRepository = stationSiteRepository;
        }

        public override void OnNavigatedTo(NavigationContext navigationContext)
        {
            _stationId = navigationContext.Parameters.GetValue<long>("StationId"); 
        }

        #region Property
        public ObservableCollection<StationSiteHardwareItemModel> StationSiteHardwareItems { get; private set; } = [];

        public ObservableCollection<HardwareListItemViewModel> HardwareListItems { get; private set; } = [];
        #endregion

        #region Command
        public AsyncDelegateCommand LoadCommand => new(ExecuteLoadCommand);
        public AsyncDelegateCommand<long> CheckedCommand => new(ExecuteCheckedCommand);

        public AsyncDelegateCommand<long> DeleteHardwareCommand => new(ExecuteDeleteHardwareCommand);
        #endregion

        #region Private Method

        private async Task ExecuteDeleteHardwareCommand(long hardwareId)
        {
            if (_stationId == -1) throw new ArgumentNullException("StationId is invalid!"); 
            foreach (var controller in HardwareListItems)
            {
                var hardware = controller.ChildItems.FirstOrDefault(s => s.HardwareId == hardwareId);
                if (hardware != null)
                {
                    hardware.IsSelectable = true;
                    hardware.IsSelected = false;
                }
            }
            var removeObj = StationSiteHardwareItems.FirstOrDefault(h => h.HardwareId == hardwareId);
            if (removeObj != null)
            {
                StationSiteHardwareItems.Remove(removeObj);
                await _stationSiteRepository.DeleteStationSiteHardwareInfosAsync(_stationId, hardwareId);
            } 
        }

        private async Task ExecuteCheckedCommand(long hardwareId)
        {
            var hardwareInfo = await _deviceInfoRepository.GetDeviceByIdAsync(hardwareId);
            if (string.IsNullOrEmpty(hardwareInfo.MainDeviceId)) return;
            var parentInfo = await _deviceInfoRepository.GetDeviceByDeviceIdAsync(hardwareInfo.MainDeviceId);
            StationSiteHardwareItems.Add(new StationSiteHardwareItemModel
            {
                ControllerName = parentInfo.DeviceName,
                HardwareName = hardwareInfo.DeviceName,
                HardwareId = hardwareId
            });
            foreach (var controller in HardwareListItems)
            {
                var itemToRemove = controller.ChildItems.FirstOrDefault(item => item.HardwareId == hardwareId);
                if (itemToRemove != null)
                {
                    itemToRemove.IsSelectable = false;
                }
            }
            await _stationSiteRepository.AddStationSiteHardwareInfosAsync([
                new StationSiteAndHardwareInfo
                {
                    StationId = _stationId,
                    HardwareId = hardwareId
                }
            ]);
        }

        private async Task ExecuteLoadCommand()
        {
            StationSiteHardwareItems.Clear();
            HardwareListItems.Clear();
            // var devices = await _deviceInfoRepository.GetAllDevicesAsync();
            if (_stationId == -1) throw new ArgumentNullException("StationId is invalid!");
            var staionSelectedHardware = await _stationSiteRepository.GetStationSiteDevices(_stationId); 
            // 右侧所有的设备
            var allDevices = await _deviceInfoRepository.GetAllDevicesAsync();
            // 左侧选中的设备
            foreach (var item in staionSelectedHardware)
            {
                var parentInfo = allDevices.FirstOrDefault(c => c.DeviceId == item.MainDeviceId);
                StationSiteHardwareItems.Add(new StationSiteHardwareItemModel
                {
                    ControllerName = parentInfo?.DeviceName ?? string.Empty,
                    HardwareName = item.DeviceName,
                    HardwareId = item.Id
                });
            }
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
                        var isSelected = staionSelectedHardware.Any(h => h.Id == c.Id);
                        return new HardwareChildItemViewModel
                        {
                            HardwareId = c.Id,
                            HardwareName = c.DeviceName,
                            IsSelectable = !isSelected,
                            IsSelected = isSelected,
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
