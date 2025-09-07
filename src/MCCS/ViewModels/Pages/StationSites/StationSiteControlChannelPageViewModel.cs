using MCCS.Core.Repositories;
using MCCS.Models.Stations;
using System.Collections.ObjectModel;
using System.Windows.Controls;
using MaterialDesignThemes.Wpf;
using MCCS.Events.Common;
using MCCS.Events.StationSites;
using MCCS.Events.StationSites.ControlChannels;
using MCCS.Views.Dialogs.Common;
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
            _eventAggregator.GetEvent<NotificationUpdateControlChannelEvent>()
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
        //public AsyncDelegateCommand<long> DeleteChannelCommand => new(ExecuteDeleteControlChannelCommand);
        public AsyncDelegateCommand LoadCommand => new(ExecuteLoadCommand);
        public AsyncDelegateCommand AddControlChannelCommand => new(ExecuteAddControlChannelCommand);
        public AsyncDelegateCommand<long> DeleteControlChannelCommand => new(ExecuteDeleteControlChannelCommand);
        public AsyncDelegateCommand<long> EditControlChannelCommand => new(ExecuteEditControlChannelCommand);
        #endregion

        #region Private Method 
        private async Task ExecuteEditControlChannelCommand(long channelId)
        {
            var editPage = _containerProvider.Resolve<EditControlChannelPage>();
            _eventAggregator.GetEvent<SendEditChannelStationSiteIdEvent>().Publish(new SendEditChannelStationSiteIdEventParam()
            {
                StationId = _stationId,
                ChannelId = channelId
            });
            await DialogHost.Show(editPage, "RootDialog");
        }
        private async Task ExecuteDeleteControlChannelCommand(long channelId)
        {
            var confirmDialog = _containerProvider.Resolve<DeleteConfirmDialog>();
            var result = await DialogHost.Show(confirmDialog, "RootDialog");
            if (result is DialogConfirmEvent { IsConfirmed: true })
            {
                var controlChannel = ControlChannels.FirstOrDefault(c => c.Id == channelId);
                if (controlChannel != null)
                {
                    ControlChannels.Remove(controlChannel);
                    await _stationSiteAggregateRepository.DeleteStationSiteControlChannelAsync(channelId);
                }
            }
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
            var allStationSiteSignals = await _stationSiteAggregateRepository.GetStationSiteDevices(_stationId);
            var controlChannels = await _stationSiteAggregateRepository.GetStationSiteControlChannels(_stationId);
            var controlChannelSignals = await _stationSiteAggregateRepository.GetControlChannelAndSignalInfosAsync(s => controlChannels.Select(c => c.Id).Contains(s.ChannelId));
            foreach (var controlChannelItem in from controlChannel in controlChannels let signals = controlChannelSignals
                         .Where(s => s.ChannelId == controlChannel.Id)
                         .Select(s =>
                         {
                             var signal = allStationSiteSignals.FirstOrDefault(c => c.Id == s.SignalId);
                             var controllerInfo = allDevices.FirstOrDefault(c => c.Id == signal?.BelongToControllerId);
                             var connectedDevice = allDevices.FirstOrDefault(c => c.Id == signal?.ConnectedDeviceId);
                             return new StationSiteControlChannelSignalViewModel
                             {
                                 SignalId = s.SignalId,
                                 SignalName = signal?.SignalName ?? "",
                                 SignalType = s.SignalType,
                                 ControllerName = controllerInfo?.DeviceName ?? "",
                                 DeviceName = connectedDevice?.DeviceName ?? ""
                                 //Address = s.Address
                             };
                         })
                         .ToList() select new StationSiteControlChannelItemViewModel
                         {
                             Id = controlChannel.Id,
                             ChannelId = controlChannel.ChannelId,
                             ChannelName = controlChannel.ChannelName,
                             ControlMode = controlChannel.ControlMode,
                             Signals = new ObservableCollection<StationSiteControlChannelSignalViewModel>(signals)
                         })
            {
                ControlChannels.Add(controlChannelItem);
            }
        }
        #endregion
    }
}
