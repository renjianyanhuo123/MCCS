using MaterialDesignThemes.Wpf; 
using MCCS.Events.Common;
using MCCS.Events.Hardwares;
using MCCS.Models.Hardwares;
using MCCS.ViewModels.Dialogs.Hardwares;
using MCCS.Views.Dialogs.Common;
using MCCS.Views.Dialogs.Hardwares;
using Serilog;
using System.Collections.ObjectModel;
using System.Windows;
using MCCS.Infrastructure.Repositories;

namespace MCCS.ViewModels.Pages.SystemManager
{
    public class HardwareSettingPageViewModel : BaseViewModel
    {
        public const string Tag = "HardwareSetting";

        private readonly IRegionManager _regionManager; 
        private readonly IDeviceInfoRepository _deviceInfoRepository;
        private readonly IContainerProvider _containerProvider;

        public HardwareSettingPageViewModel(
            IRegionManager regionManager,
            IEventAggregator eventAggregator,
            IDialogService dialogService,
            IContainerProvider containerProvider,
            IDeviceInfoRepository deviceInfoRepository) : base(eventAggregator, dialogService)
        {
            _regionManager = regionManager;
            _deviceInfoRepository = deviceInfoRepository;
            _containerProvider = containerProvider;
            _eventAggregator.GetEvent<NotificationAddHardwareEvent>().Subscribe(AddHardware_Refresh);
            _eventAggregator.GetEvent<NotificationEditHardwareEvent>().Subscribe(UpdateHardware_Refresh);
            //_eventAggregator.GetEvent<NotificationUpdateVariableEvent>().Subscribe(UpdateVariable_RefreshTreeView);
        }

        #region Command 
        public AsyncDelegateCommand LoadCommand => new(ExecuteLoadCommand); 
        public AsyncDelegateCommand<long> EditHardwareCommand => new(ExecuteEditHardwareCommand);
        public AsyncDelegateCommand<long> DeleteHardwareCommand => new(ExecuteDeleteHardwareCommand);
        public AsyncDelegateCommand AddHardwareCommand => new(ExecuteAddHardwareCommand);
        public AsyncDelegateCommand<long> EditSignalCommand => new(ExecuteEditSignalCommand);
        #endregion

        #region Property 
        private ObservableCollection<HardwareListItemViewModel> _hardwareList = [];
        public ObservableCollection<HardwareListItemViewModel> HardwareList
        {
            get => _hardwareList; 
            set => SetProperty(ref _hardwareList, value);
        } 
        #endregion

        #region private method
        private async Task ExecuteEditHardwareCommand(long id)
        {
            var editDialog = _containerProvider.Resolve<EditHardwareDialog>();
            _eventAggregator.GetEvent<SendHardwareIdEvent>().Publish(new SendHardwareIdEventParam() { HardwareId = id });
            var result = await DialogHost.Show(editDialog, "RootDialog");
        }
        private async Task ExecuteAddHardwareCommand()
        {
            var addDialog = _containerProvider.Resolve<AddHardwareDialog>();
            var result = await DialogHost.Show(addDialog, "RootDialog");
        }
        private async Task ExecuteDeleteHardwareCommand(long id)
        {
            var confirmDialog = _containerProvider.Resolve<DeleteConfirmDialog>();
            var result = await DialogHost.Show(confirmDialog, "RootDialog");
            if (result is DialogConfirmEvent { IsConfirmed: true })
            {
                // Execute delete logic here
                var success = await _deviceInfoRepository.DeleteDeviceInfoAsync(id);
                if (success)
                {
                    await ExecuteLoadCommand();
                }
            }
        }
        private async Task ExecuteEditSignalCommand(long id)
        {
            var editDialog = _containerProvider.Resolve<EditSignalDialog>(); 
            _eventAggregator.GetEvent<SendHardwareSignalIdEvent>().Publish(new SendHardwareSignalIdEventParam{ControllerId = id});
            var result = await DialogHost.Show(editDialog, "RootDialog"); 
        }  

        private void ExecuteSelectedCommand(RoutedPropertyChangedEventArgs<object> param)
        {
            //var parameters = new NavigationParameters(); 
            //switch (param.NewValue)
            //{
            //    case ChannelVariableInfoViewModel channel: 
            //        parameters.Add("ChannelId", channel.ChannelId);
            //        _regionManager.RequestNavigate(GlobalConstant.SystemManagerHardwarePageRegionName, new Uri(ChannelSettingPageViewModel.Tag, UriKind.Relative), parameters);
            //        break;
            //    case VariableInfoViewModel variable:
            //        parameters.Add("VariableId", variable.VariableId);
            //        _regionManager.RequestNavigate(GlobalConstant.SystemManagerHardwarePageRegionName, new Uri(VariableSettingPageViewModel.Tag, UriKind.Relative), parameters);
            //        break;
            //    default:
            //        // Debug.WriteLine("Unknown type selected");
            //        break;
            //}

            // Debug.WriteLine(param);
        }

        private async Task ExecuteLoadCommand()
        {
            HardwareList.Clear();
            var hardwareList = await _deviceInfoRepository.GetAllDevicesAsync();
            var hardwareListView = hardwareList.Select(s => new HardwareListItemViewModel()
            {
                Id = s.Id,
                Name = s.DeviceName,
                Description = s.Description ?? "",
                Type = s.DeviceType,
                CreateTime = s.CreateTime.ToString("yyyy-MM-dd hh:mm:ss"),
                UpdateTime = s.UpdateTime.ToString("yyyy-MM-dd hh:mm:ss"),
            });
            HardwareList = new ObservableCollection<HardwareListItemViewModel>(hardwareListView);
        }

        private async void AddHardware_Refresh(NotificationAddHardwareEventParam param)
        {
            try
            {
                await ExecuteLoadCommand(); 
            }
            catch (Exception e)
            {
                Log.Error($"添加设备后刷新失败！{e.Message}");
            }
        } 
        private async void UpdateHardware_Refresh(NotificationEditHardwareEventParam param)
        {
            try
            {
                await ExecuteLoadCommand();
            }
            catch (Exception e)
            {
                Log.Error($"更新设备后刷新失败！{e.Message}");
            }
        }
        #endregion
    }
}
