using System.Collections.ObjectModel;

using MCCS.Common.Resources.ViewModels;
using MCCS.Infrastructure.Models.Devices;
using MCCS.Infrastructure.Models.StationSites;
using MCCS.Infrastructure.Repositories;
using MCCS.Models.Stations;

namespace MCCS.ViewModels.Pages.StationSites
{
    public sealed class StationSiteHardwarePageViewModel(
        IEventAggregator eventAggregator,
        IDeviceInfoRepository deviceInfoRepository,
        IStationSiteAggregateRepository stationSiteRepository)
        : BaseViewModel(eventAggregator)
    {
        public const string Tag = "StationSiteHardware";

        private long _stationId = -1;

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

        private async Task ExecuteDeleteHardwareCommand(long signalId)
        {
            if (_stationId == -1) throw new ArgumentNullException("StationId is invalid!"); 
            foreach (var controller in HardwareListItems)
            {
                var hardware = controller.ChildItems.FirstOrDefault(s => s.SignalId == signalId);
                if (hardware != null)
                {
                    hardware.IsSelectable = true;
                    hardware.IsSelected = false;
                }
            }
            var removeObj = StationSiteHardwareItems.FirstOrDefault(h => h.SignalId == signalId);
            if (removeObj != null)
            {
                StationSiteHardwareItems.Remove(removeObj);
                await stationSiteRepository.DeleteStationSiteHardwareInfosAsync(_stationId, signalId);
            } 
        }

        private async Task ExecuteCheckedCommand(long signalId)
        {
            var signalInfo = await deviceInfoRepository.GetSignalInterfaceByIdAsync(signalId);
            if (signalInfo == null) return;
            if (signalInfo.BelongToControllerId <= 0) return;
            var parentInfo = await deviceInfoRepository.GetDeviceByIdAsync(signalInfo.BelongToControllerId);
            var hardwareInfo = await deviceInfoRepository.GetDeviceByIdAsync(signalInfo.ConnectedDeviceId);
            StationSiteHardwareItems.Add(new StationSiteHardwareItemModel
            {
                ControllerName = parentInfo.DeviceName,
                SignalId = signalInfo.Id,
                SignalName = signalInfo.SignalName,
                HardwareName = hardwareInfo?.DeviceName ?? "",
                HardwareId = hardwareInfo?.Id ?? 0
            });
            foreach (var controller in HardwareListItems)
            {
                var itemToRemove = controller.ChildItems.FirstOrDefault(item => item.SignalId == signalId);
                if (itemToRemove != null)
                {
                    itemToRemove.IsSelectable = false;
                }
            }
            await stationSiteRepository.AddStationSiteHardwareInfosAsync([
                new StationSiteAndHardwareInfo
                {
                    StationId = _stationId,
                    SignalId = signalId,
                    HardwareId = hardwareInfo?.Id ?? 0
                }
            ]);
        }

        private async Task ExecuteLoadCommand()
        {
            StationSiteHardwareItems.Clear();
            HardwareListItems.Clear();
            // var devices = await _deviceInfoRepository.GetAllDevicesAsync();
            if (_stationId == -1) throw new ArgumentNullException("StationId is invalid!");
            var stationSelectedHardware = await stationSiteRepository.GetStationSiteDevices(_stationId);  
            var allDevices = await deviceInfoRepository.GetAllDevicesAsync();
            var allSignals = await deviceInfoRepository.GetSignalInterfacesByExpressionAsync(c => allDevices.Select(s => s.Id).Contains(c.ConnectedDeviceId));
            // 左侧选中的设备
            foreach (var item in stationSelectedHardware)
            {
                var parentInfo = allDevices.FirstOrDefault(c => c.Id == item.BelongToControllerId);
                var connectedDevice = allDevices.FirstOrDefault(c => c.Id == item.ConnectedDeviceId);
                StationSiteHardwareItems.Add(new StationSiteHardwareItemModel
                {
                    ControllerName = parentInfo?.DeviceName ?? string.Empty,
                    HardwareName = connectedDevice?.DeviceName ?? "",
                    HardwareId = connectedDevice?.Id ?? 0,
                    SignalId = item.Id,
                    SignalName = item.SignalName
                });
            }
            // 右侧所有的设备
            var parentInfos = allDevices
                .Where(s => s.DeviceType == DeviceTypeEnum.Controller)
                .Distinct()
                .ToList(); 
            foreach (var parentInfo in parentInfos)
            {
                var hardwareListItem = new HardwareListItemViewModel
                {
                    ControllerId = parentInfo.Id,
                    ControllerName = parentInfo.DeviceName,
                };
                var childItems = allSignals
                    .Where(c => c.BelongToControllerId == parentInfo.Id)
                    .Select(c =>
                    {
                        var isSelected = StationSiteHardwareItems.Any(h => h.SignalId == c.Id);
                        var device = allDevices.FirstOrDefault(d => d.Id == c.ConnectedDeviceId);
                        return new HardwareChildItemViewModel
                        {
                            HardwareId = device?.Id ?? 0,
                            HardwareName = device?.DeviceName ?? "",
                            SignalId = c.Id,
                            SignalName = c.SignalName,
                            IsSelectable = !isSelected,
                            IsSelected = isSelected
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
