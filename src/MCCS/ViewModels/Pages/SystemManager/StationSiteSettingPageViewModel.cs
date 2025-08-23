using System.Collections.ObjectModel;
using MaterialDesignThemes.Wpf;
using MCCS.Core.Repositories;
using MCCS.Events.StationSites;
using MCCS.Models.Stations;
using MCCS.Views.Dialogs;

namespace MCCS.ViewModels.Pages.SystemManager
{
    public class StationSiteSettingPageViewModel : BaseViewModel
    {
        public const string Tag = "StationSiteSetting";

        private readonly IStationSiteRepository _stationSiteRepository;
        private readonly IContainerProvider _containerProvider;

        public StationSiteSettingPageViewModel(IEventAggregator eventAggregator,
            IStationSiteRepository stationSiteRepository,
            IContainerProvider containerProvider) : base(eventAggregator)
        {
            _stationSiteRepository = stationSiteRepository;
            _containerProvider = containerProvider;
            _eventAggregator.GetEvent<NotificationAddStationSiteEvent>().Subscribe(ExecuteNotificationAddStationEvent);
            StationSites = [];
        }

        #region Property
        public ObservableCollection<StationSiteInfoViewModel> StationSites { get; set; }
        #endregion

        #region Command
        public AsyncDelegateCommand LoadCommand => new(ExecuteLoadCommand);
        public AsyncDelegateCommand AddStationCommand => new(ExecuteAddStationCommand);
        #endregion

        #region Private Method 
        private async void ExecuteNotificationAddStationEvent(NotificationAddStationSiteEventParam param)
        {
            await ExecuteLoadCommand();
        }

        private async Task ExecuteLoadCommand()
        {
            StationSites.Clear();
            var list = await _stationSiteRepository.GetStationSitesAsync(c => true); 
            foreach (var item in list)
            {
                StationSites.Add(new StationSiteInfoViewModel
                { 
                    StationId = item.Id,
                    StationName = item.StationName,
                    Description = item.Description,
                    IsUsing = item.IsUsing,
                    CreateTime = item.CreateTime.ToString("yyyy-MM-dd hh:mm:ss"),
                    UpdateTime = item.UpdateTime.ToString("yyyy-MM-dd hh:mm:ss")
                });
            }
        }

        private async Task ExecuteAddStationCommand()
        {
            var addStationDialog = _containerProvider.Resolve<AddStationSiteInfoDialog>();
            var result = await DialogHost.Show(addStationDialog, "RootDialog");
        } 
        #endregion
    }
}
