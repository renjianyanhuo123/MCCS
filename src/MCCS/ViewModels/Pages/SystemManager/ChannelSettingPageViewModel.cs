using MCCS.Models.Stations;
using MCCS.ViewModels.Others.SystemManager;
using System.Collections.ObjectModel;

using MCCS.Common.Resources.Extensions;
using MCCS.Common.Resources.ViewModels;
using MCCS.Infrastructure.Repositories;

namespace MCCS.ViewModels.Pages.SystemManager
{
    public class ChannelSettingPageViewModel : BaseViewModel
    {
        public const string Tag = "ChannelSetting";

        private readonly IDeviceInfoRepository _deviceInfoRepository;
        private readonly INotificationService _notificationService;

        public ChannelSettingPageViewModel(
            INotificationService notificationService,
            IDeviceInfoRepository deviceInfoRepository,
            IEventAggregator eventAggregator) : base(eventAggregator)
        {
            _deviceInfoRepository = deviceInfoRepository;
            _notificationService = notificationService;
            _channelName = string.Empty; // CS8618: 显式初始化为非 null 值
        }

        #region Property
        public long ChannelId { get; set; }

        private string _internalChannelName = string.Empty;
        public string InternalChannelName
        {
            get => _internalChannelName;
            set => SetProperty(ref _internalChannelName, value);
        }

        private string _channelName = string.Empty; // CS8618: 显式初始化为非 null 值
        public string ChannelName
        {
            get => _channelName;
            set => SetProperty(ref _channelName, value);
        }

        private bool _isShowable;
        public bool IsShowable
        {
            get => _isShowable;
            set => SetProperty(ref _isShowable, value);
        }

        private bool _isOpenProtected;
        public bool IsOpenProtected
        {
            get => _isOpenProtected;
            set => SetProperty(ref _isOpenProtected, value);
        }

        public ObservableCollection<ChannelHardwareViewModel> ChannelHardwareInfo { get; private set; } = [];
        public ObservableCollection<HardwareListItemViewModel> HardwareList { get; private set; } = [];
        #endregion

        #region Command
        public AsyncDelegateCommand<long> DeleteHardwareCommand => new(ExecuteDeleteHardwareCommand);
        public AsyncDelegateCommand<long> CheckedCommand => new(ExecuteCheckedCommand);
        public AsyncDelegateCommand SaveCommand => new(ExecuteSaveCommand);
        #endregion

        #region private method
        private void InitEditUi(long channelId)
        {
            //var channelInfo = _channelAggregateRepository.GetChannelInfoById(channelId);
            //ChannelId = channelInfo.Id;
            //InternalChannelName = channelInfo.ChannelId;
            //ChannelName = channelInfo.ChannelName;
            //IsShowable = channelInfo.IsShowable;
            //IsOpenProtected = channelInfo.IsOpenSpecimenProtected;
        }

        private async Task ExecuteSaveCommand()
        {
            //if (string.IsNullOrEmpty(ChannelName)) return;
            //var success = await _channelAggregateRepository.UpdateChannelInfoAsync(ChannelId, ChannelName, IsShowable, IsOpenProtected);
            //if (success)
            //{
            //    _notificationService.Show("保存成功!", "通道基础信息保存成功", NotificationType.Success, 3);
            //    _eventAggregator.GetEvent<NotificationUpdateChannelEvent>().Publish(new NotificationUpdateChannelEventParam(ChannelId));
            //}
        }

        public override void OnNavigatedTo(NavigationContext navigationContext)
        {
            //var channelId = navigationContext.Parameters.GetValue<long>("ChannelId"); 
            //InitEditUi(channelId);
            //// 左侧所有已经在对应通道内的设备
            //var hardwareInfos = _channelAggregateRepository.GetHardwareInfoByChannelId(channelId); 
            //ChannelHardwareInfo.Clear();
            //HardwareList.Clear();
            //// 右侧所有的设备
            //var allDevices = _deviceInfoRepository.GetDevicesByExpression(c => true);
            //var parentIds = allDevices
            //    .Where(s => s.MainDeviceId != null)
            //    .Select(s => s.MainDeviceId)
            //    .Distinct()
            //    .ToList();
            //var selectedHardwareIds = _channelAggregateRepository.GetAllChannelHardwareIds();
            //var parentInfos = allDevices
            //    .Where(c => parentIds.Contains(c.DeviceId))
            //    .ToList();
            //foreach (var parentInfo in parentInfos)
            //{
            //    var hardwareListItem = new HardwareListItemViewModel
            //    {
            //        ControllerId = parentInfo.Id,
            //        ControllerName = parentInfo.DeviceName,
            //    };
            //    var childItems = allDevices
            //        .Where(c => c.MainDeviceId == parentInfo.DeviceId)
            //        .Select(c =>
            //        {
            //            var isSelected = selectedHardwareIds.Any(h => h == c.Id);
            //            return new HardwareChildItemViewModel
            //            {
            //                HardwareId = c.Id,
            //                HardwareName = c.DeviceName,
            //                IsSelected = isSelected,
            //                IsSelectable = !isSelected,
            //                DeviceStatus = DeviceStatusEnum.Connected,
            //            };
            //        }).ToList(); 
            //    foreach (var item in childItems)
            //    {
            //        hardwareListItem.ChildItems.Add(item);
            //    }
            //    HardwareList.Add(hardwareListItem);
            //}
            //foreach (var hardware in hardwareInfos)
            //{
            //    if (hardware.MainDeviceId == null) continue;
            //    ChannelHardwareInfo.Add(new ChannelHardwareViewModel
            //    {
            //        ControllerName = parentInfos.FirstOrDefault(c => c.DeviceId == hardware.MainDeviceId)?.DeviceName ?? string.Empty,
            //        HardwareName = hardware.DeviceName,
            //        HardwareId = hardware.Id,
            //    });
            //}

        }

        private async Task ExecuteCheckedCommand(long hardwareId)
        {
            //var hardwareInfo = await _deviceInfoRepository.GetDeviceByIdAsync(hardwareId);
            //if (string.IsNullOrEmpty(hardwareInfo.MainDeviceId)) return;
            //var parentInfo = await _deviceInfoRepository.GetDeviceByDeviceIdAsync(hardwareInfo.MainDeviceId);
            //ChannelHardwareInfo.Add(new ChannelHardwareViewModel
            //{
            //    ControllerName = parentInfo.DeviceName,
            //    HardwareName = hardwareInfo.DeviceName,
            //    HardwareId = hardwareId
            //});
            //foreach (var controller in HardwareList)
            //{
            //    foreach (var item in controller.ChildItems)
            //    {
            //        if (item.HardwareId != hardwareId) continue;
            //        item.IsSelectable = false;
            //        break;
            //    }
            //}
            //await _channelAggregateRepository.AddChannelHardware(ChannelId, hardwareId);
        }

        private async Task ExecuteDeleteHardwareCommand(long hardwareId)
        {
            //var removeObj = ChannelHardwareInfo.FirstOrDefault(s => s.HardwareId == hardwareId);
            //if (removeObj == null) return;
            //ChannelHardwareInfo.Remove(removeObj);
            //await _channelAggregateRepository.DeleteChannelHardware(ChannelId, hardwareId); 
            //foreach (var controller in HardwareList)
            //{
            //    foreach (var item in controller.ChildItems)
            //    {
            //        if (item.HardwareId != hardwareId) continue;
            //        item.IsSelected = false;
            //        item.IsSelectable = true;
            //        break;
            //    }
            //}
        }

        #endregion
    }
}
