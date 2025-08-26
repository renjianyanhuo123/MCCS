using System.Collections.ObjectModel;
using MaterialDesignThemes.Wpf;
using MCCS.Core.Repositories;
using MCCS.Events.StationSites;
using MCCS.Models.Stations;
using MCCS.ViewModels.Pages.StationSites;
using MCCS.Views.Dialogs;
using Serilog;

namespace MCCS.ViewModels.Pages.SystemManager
{
    public class StationSiteSettingPageViewModel : BaseViewModel
    {
        public const string Tag = "StationSiteSetting";

        private readonly IStationSiteRepository _stationSiteRepository;
        private readonly IContainerProvider _containerProvider;
        private readonly IRegionManager _regionManager;

        public StationSiteSettingPageViewModel(IEventAggregator eventAggregator,
            IStationSiteRepository stationSiteRepository,
            IContainerProvider containerProvider,
            IRegionManager regionManager) : base(eventAggregator)
        {
            _stationSiteRepository = stationSiteRepository;
            _containerProvider = containerProvider;
            _eventAggregator.GetEvent<NotificationAddStationSiteEvent>().Subscribe(ExecuteNotificationAddStationEvent);
            StationSites = [];
            _regionManager = regionManager;
        }

        #region Property
        public ObservableCollection<StationSiteInfoViewModel> StationSites { get; set; }
        #endregion

        #region Command
        public AsyncDelegateCommand LoadCommand => new(ExecuteLoadCommand);
        public AsyncDelegateCommand AddStationCommand => new(ExecuteAddStationCommand);
        public DelegateCommand<object> EditStationCommand => new(ExecuteEditStationCommand);
        #endregion

        #region Private Method
        private void ExecuteEditStationCommand(object stationId)
        {
            var paramters = new NavigationParameters { { "StationId", stationId } };
            _regionManager.RequestNavigate(GlobalConstant.SystemManagerRegionName, new Uri(EditStationSiteMainPageViewModel.Tag, UriKind.Relative), Test, paramters);
        }

        private void Test(NavigationResult res)
        {
        }

        private async void ExecuteNotificationAddStationEvent(NotificationAddStationSiteEventParam param)
        {
            try
            {
                await ExecuteLoadCommand();
            }
            catch (Exception e)
            {
                Log.Error(e, "Error occurred while loading station sites after adding a new station site.");
            }
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
