using MCCS.Core.Repositories;
using MCCS.Models.Stations;
using System.Collections.ObjectModel;
using MaterialDesignThemes.Wpf;
using MCCS.Events.StationSites;
using MCCS.Events.StationSites.ControlChannels;
using MCCS.Views.Pages.StationSites.ControlChannels;

namespace MCCS.ViewModels.Pages.StationSites
{
    public sealed class StationSiteControlChannelPageViewModel : BaseViewModel
    {
        public const string Tag = "StationSiteControlChannel";

        private long _stationId = -1;
        private readonly IStationSiteAggregateRepository _stationSiteAggregateRepository;
        private readonly IDeviceInfoRepository _deviceInfoRepository;
        private readonly IContainerProvider _containerProvider;

        public StationSiteControlChannelPageViewModel(IEventAggregator eventAggregator,
            IStationSiteAggregateRepository stationSiteAggregateRepository,
            IDeviceInfoRepository deviceInfoRepository,
            IContainerProvider containerProvider) : base(eventAggregator)
        {
            _stationSiteAggregateRepository = stationSiteAggregateRepository;
            _deviceInfoRepository = deviceInfoRepository;
            _containerProvider = containerProvider;
            _eventAggregator.GetEvent<NotificationAddControlChannelEvent>()
                .Subscribe(async param => await ExecuteLoadCommand());
        }

        public override void OnNavigatedTo(NavigationContext navigationContext)
        {
            _stationId = navigationContext.Parameters.GetValue<long>("StationId");
        }

        #region Property 
        public ObservableCollection<ControlChannelHardwareListItemViewModel> HardwareListItems { get; private set; } = [];
        public ObservableCollection<StationSiteControlChannelItemViewModel> ControlChannels { get; private set; } = []; 
        #endregion

        #region Command
        public AsyncDelegateCommand LoadCommand => new(ExecuteLoadCommand);
        public AsyncDelegateCommand AddControlChannelCommand => new(ExecuteAddControlChannelCommand);
        public DelegateCommand<string> DeleteControlChannelCommand => new(ExecuteDeleteControlChannelCommand);
        #endregion

        #region Private Method 
        private void ExecuteDeleteControlChannelCommand(string channelId)
        {
            var controlChannel = ControlChannels.FirstOrDefault(c => c.ChannelId == channelId);
            if(controlChannel != null) ControlChannels.Remove(controlChannel);
        }

        private async Task ExecuteAddControlChannelCommand()
        {
            var addDialog = _containerProvider.Resolve<AddControlChannelPage>();
            _eventAggregator.GetEvent<SendStationSiteIdEvent>().Publish(new SendStationSiteIdEventParam()
            {
                StationId = _stationId
            });
            await DialogHost.Show(addDialog, "RootDialog");
        }

        private async Task ExecuteLoadCommand()
        {
            // Load control channel hardware list items from data source
            if(_stationId == -1) throw new InvalidOperationException("StationId is not set.");
            HardwareListItems.Clear();
            ControlChannels.Clear();
            var allDevices = await _deviceInfoRepository.GetAllDevicesAsync();
            var allStationSiteHardwares = await _stationSiteAggregateRepository.GetStationSiteDevices(_stationId);
            var controlChannels = await _stationSiteAggregateRepository.GetStationSiteControlChannels(_stationId);
            var controlChannelSignals = await _stationSiteAggregateRepository.GetControlChannelAndSignalInfosAsync(s => controlChannels.Select(c => c.Id).Contains(s.ChannelId));
            foreach (var controlChannel in controlChannels)
            {
                var signals = controlChannelSignals
                    .Where(s => s.ChannelId == controlChannel.Id)
                    .Select(s => new StationSiteControlChannelSignalViewModel
                    {
                        SignalId = s.SignalId,
                        //SignalName = s.SignalName,
                        SignalType = s.SignalType,
                        //Address = s.Address
                    })
                    .ToList();
                var controlChannelItem = new StationSiteControlChannelItemViewModel
                {
                    Id = controlChannel.Id,
                    ChannelId = controlChannel.ChannelId,
                    ChannelName = controlChannel.ChannelName,
                    ControlMode = controlChannel.ControlMode,
                };
                // controlChannelItem.Signals
                ControlChannels.Add(controlChannelItem);
            }
            // 所有的控制通道
            //var parentIds = allDevices
            //    .Where(s => s.MainDeviceId != null)
            //    .Select(s => s.MainDeviceId)
            //    .Distinct()
            //    .ToList();
            //var parentInfos = allDevices
            //    .Where(c => parentIds.Contains(c.DeviceId))
            //    .ToList();
            //foreach (var parentInfo in parentInfos)
            //{
            //    var hardwareListItem = new ControlChannelHardwareListItemViewModel()
            //    {
            //        ControllerId = parentInfo.Id,
            //        ControllerName = parentInfo.DeviceName,
            //    };
            //    var childItems = allDevices
            //        .Where(c => c.MainDeviceId == parentInfo.DeviceId 
            //                    && allStationSiteHardwares
            //                        .Select(s => s.Id)
            //                        .Contains(c.Id))
            //        .ToList();
            //    foreach (var item in childItems)
            //    {
            //        hardwareListItem.ChildItems.Add(new ControlChannelHardwareChildItemViewModel
            //        {
            //            HardwareId = item.Id,
            //            HardwareName = item.DeviceName,
            //            Address = "TCU.1 MCU2.1 9232"
            //        });
            //    }
            //    HardwareListItems.Add(hardwareListItem);
            //}
        }
        #endregion
    }
}
