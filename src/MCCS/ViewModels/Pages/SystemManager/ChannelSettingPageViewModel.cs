using System.Collections.ObjectModel;
using MCCS.Core.Models.Devices;
using MCCS.Core.Repositories;
using MCCS.ViewModels.Others.SystemManager;

namespace MCCS.ViewModels.Pages.SystemManager
{
    public class ChannelSettingPageViewModel:BaseViewModel
    {
        public const string Tag = "ChannelSetting";

        private long _currentChannelId = 0;

        private readonly IChannelAggregateRepository _channelAggregateRepository;
        private readonly IDeviceInfoRepository _deviceInfoRepository;

        public ChannelSettingPageViewModel(
            IDeviceInfoRepository deviceInfoRepository,
            IChannelAggregateRepository channelAggregateRepository,
            IEventAggregator eventAggregator) : base(eventAggregator)
        {
            _deviceInfoRepository = deviceInfoRepository;
            _channelAggregateRepository = channelAggregateRepository;
        }

        #region Property 
        public ObservableCollection<ChannelHardwareViewModel> ChannelHardwareInfo { get; private set; } = [];
        public ObservableCollection<HardwareListItemViewModel> HardwareList { get; private set; } = [];
        #endregion

        #region Command
        // public AsyncDelegateCommand LoadedCommand => new(ExecuteLoadedCommand);
        public AsyncDelegateCommand<long> DeleteHardwareCommand => new(ExecuteDeleteHardwareCommand);
        public AsyncDelegateCommand<long> CheckedCommand => new(ExecuteCheckedCommand);
        #endregion

        #region private method 

        public override void OnNavigatedTo(NavigationContext navigationContext)
        {
            var channelId = navigationContext.Parameters.GetValue<long>("ChannelId");
            _currentChannelId = channelId;
            var hardwareInfos = _channelAggregateRepository.GetHardwareInfoByChannelId(channelId);
            if (hardwareInfos == null) throw new ArgumentNullException(nameof(hardwareInfos));
            ChannelHardwareInfo.Clear();
            HardwareList.Clear();
            var parentIds = hardwareInfos
                .Where(s => s.MainDeviceId != null)
                .Select(s => s.MainDeviceId)
                .Distinct()
                .ToList();
            var allDevices = _deviceInfoRepository.GetDevicesByExpression(c => true);
            var selectedHardwareIds = _channelAggregateRepository.GetAllChannelHardwareIds();
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
                            IsSelected = isSelected,
                            IsSelectable = !isSelected,
                            DeviceStatus = DeviceStatusEnum.Connected,
                        };
                    }).ToList(); 
                foreach (var item in childItems)
                {
                    hardwareListItem.ChildItems.Add(item);
                }
                HardwareList.Add(hardwareListItem);
            }
            foreach (var hardware in hardwareInfos)
            {
                if (hardware.MainDeviceId == null) continue;
                ChannelHardwareInfo.Add(new ChannelHardwareViewModel
                {
                    ControllerName = parentInfos.FirstOrDefault(c => c.DeviceId == hardware.MainDeviceId)?.DeviceName ?? string.Empty,
                    HardwareName = hardware.DeviceName,
                    HardwareId = hardware.Id,
                });
            }

        }

        private async Task ExecuteCheckedCommand(long hardwareId)
        {
            var hardwareInfo = await _deviceInfoRepository.GetDeviceByIdAsync(hardwareId);
            if (string.IsNullOrEmpty(hardwareInfo.MainDeviceId)) return;
            var parentInfo = await _deviceInfoRepository.GetDeviceByDeviceIdAsync(hardwareInfo.MainDeviceId);
            ChannelHardwareInfo.Add(new ChannelHardwareViewModel
            {
                ControllerName = parentInfo.DeviceName,
                HardwareName = hardwareInfo.DeviceName,
                HardwareId = hardwareId
            });
            foreach (var controller in HardwareList)
            {
                foreach (var item in controller.ChildItems)
                {
                    if (item.HardwareId != hardwareId) continue;
                    item.IsSelectable = false;
                    break;
                }
            }
            await _channelAggregateRepository.AddChannelHardware(_currentChannelId, hardwareId);
        }

        private async Task ExecuteDeleteHardwareCommand(long hardwareId)
        {
            var removeObj = ChannelHardwareInfo.FirstOrDefault(s => s.HardwareId == hardwareId);
            if (removeObj == null) return;
            ChannelHardwareInfo.Remove(removeObj);
            await _channelAggregateRepository.DeleteChannelHardware(_currentChannelId, hardwareId); 
            foreach (var controller in HardwareList)
            {
                foreach (var item in controller.ChildItems)
                {
                    if (item.HardwareId != hardwareId) continue;
                    item.IsSelected = false;
                    item.IsSelectable = true;
                    break;
                }
            }
        }

        #endregion
    }
}
