using System.Collections.ObjectModel; 
using MCCS.Models.Stations.PseudoChannels;
using MaterialDesignThemes.Wpf;

using MCCS.Common.Resources.ViewModels;
using MCCS.Common.Resources.Views;
using MCCS.Events.StationSites;
using MCCS.Events.StationSites.PseudoChannels;
using MCCS.Views.Pages.StationSites.PseudoChannels;
using MCCS.Events.Common;
using MCCS.Infrastructure.Repositories;
using Serilog;

namespace MCCS.ViewModels.Pages.StationSites
{
    public sealed class StationSitePseudoChannelPageViewModel : BaseViewModel
    {
        public const string Tag = "StationSitePseudoChannel";

        private readonly IStationSiteAggregateRepository _stationSiteAggregateRepository;
        private readonly IContainerProvider _containerProvider;

        private long _stationId = -1;

        public StationSitePseudoChannelPageViewModel(IStationSiteAggregateRepository stationSiteAggregateRepository,
            IEventAggregator eventAggregator,
            IContainerProvider containerProvider) : base(eventAggregator)
        {
            _stationSiteAggregateRepository = stationSiteAggregateRepository;
            _containerProvider = containerProvider;
            AddPseudoChannelCommand = new AsyncDelegateCommand(ExecuteAddPseudoChannelCommand);
            EditPseudoChannelCommand = new AsyncDelegateCommand<long>(ExecuteEditPseudoChannelCommand);
            DeletePseudoChannelCommand = new AsyncDelegateCommand<long>(ExecuteDeletePseudoChannelCommand);
            LoadCommand = new AsyncDelegateCommand(ExecuteLoadCommand);

            _eventAggregator.GetEvent<NotificationAddPseudoChannelEvent>()
                .Subscribe(AddPseudoChannel_Refresh);
            _eventAggregator.GetEvent<NotificationUpdatePseudoChannelEvent>()
                .Subscribe(UpdatePseudoChannel_Refresh);
        }

        private async void AddPseudoChannel_Refresh(NotificationAddPseudoChannelEventParam param)
        {
            try
            {
                await ExecuteLoadCommand();
            }
            catch (Exception e)
            {
                Log.Error($"添加通道后刷新失败！{e.Message}");
            }
        }

        private async void UpdatePseudoChannel_Refresh(NotificationUpdatePseudoChannelEventParam param)
        {
            try
            {
                await ExecuteLoadCommand();
            }
            catch (Exception e)
            {
                Log.Error($"添加通道后刷新失败！{e.Message}");
            }
        }

        public override void OnNavigatedTo(NavigationContext navigationContext)
        {
            _stationId = navigationContext.Parameters.GetValue<long>("StationId");
        }

        public ObservableCollection<PseudoChannelListItemViewModel> Channels { get; } = [];

        #region Command

        public AsyncDelegateCommand LoadCommand { get; }

        public AsyncDelegateCommand AddPseudoChannelCommand { get; }

        public AsyncDelegateCommand<long> EditPseudoChannelCommand { get; }

        public AsyncDelegateCommand<long> DeletePseudoChannelCommand { get; }
        #endregion

        #region Private Method
        private async Task ExecuteAddPseudoChannelCommand()
        {
            var addDialog = _containerProvider.Resolve<AddPseudoChannelPage>();
            _eventAggregator.GetEvent<SendStationSiteIdEvent>().Publish(new SendStationSiteIdEventParam()
            {
                StationId = _stationId
            });
            await DialogHost.Show(addDialog, "RootDialog");
        }

        private async Task ExecuteEditPseudoChannelCommand(long channelId)
        {
            var editPage = _containerProvider.Resolve<EditPseudoChannelPage>();
            _eventAggregator.GetEvent<SendEditPseudoChannelStationSiteIdEvent>().Publish(new SendEditPseudoChannelStationSiteIdEventParam()
            {
                StationId = _stationId,
                ChannelId = channelId
            });
            await DialogHost.Show(editPage, "RootDialog");
        }

        private async Task ExecuteDeletePseudoChannelCommand(long channelId)
        {
            var confirmDialog = _containerProvider.Resolve<DeleteConfirmDialog>();
            var result = await DialogHost.Show(confirmDialog, "RootDialog");
            if (result is DialogConfirmEvent { IsConfirmed: true })
            {
                var pseudoChannel = Channels.FirstOrDefault(c => c.Id == channelId);
                if (pseudoChannel != null)
                {
                    Channels.Remove(pseudoChannel);
                    await _stationSiteAggregateRepository.DeleteStationSitePseudoChannelAsync(channelId);
                }
            }
        }

        private async Task ExecuteLoadCommand()
        {
            if (_stationId == -1) throw new InvalidOperationException("StationId is not set.");
            Channels.Clear();
            var pseudoChannels = await _stationSiteAggregateRepository.GetStationSitePseudoChannels(_stationId);
            foreach (var channel in pseudoChannels)
            {
                Channels.Add(new PseudoChannelListItemViewModel
                {
                    Id = channel.Id,
                    Name = channel.ChannelName,
                    Unit = channel.Unit ?? "",
                    CreateTime = channel.CreateTime.ToString("yyyy-MM-dd HH:mm:ss"),
                    UpdateTime = channel.UpdateTime.ToString("yyyy-MM-dd HH:mm:ss")
                });
            }
        }
        #endregion
    }
}
