using System.Collections.ObjectModel;
using MCCS.Models.Stations;

namespace MCCS.ViewModels.Pages.StationSites
{
    public sealed class EditStationSiteMainPageViewModel : BaseViewModel
    {
        public const string Tag = "EditStationSiteMainPage";

        private readonly IRegionManager _regionManager;
        private long _stationId = -1;

        public EditStationSiteMainPageViewModel(IEventAggregator eventAggregator, 
            IRegionManager regionManager) : base(eventAggregator)
        {
            MenuItems =
            [
                new StationMenuItemModel { Name = "硬件概述", Id = 1 },
                new StationMenuItemModel { Name = "控制通道", Id = 2 },
                new StationMenuItemModel { Name = "液压子站", Id = 3 },
                new StationMenuItemModel { Name = "虚拟通道", Id = 4 },
                new StationMenuItemModel { Name = "模型配置", Id = 5 }
            ];
            _regionManager = regionManager;
        }

        public override void OnNavigatedTo(NavigationContext navigationContext)
        {
            _stationId = navigationContext.Parameters.GetValue<long>("StationId");
        }

        #region Property
        public ObservableCollection<StationMenuItemModel> MenuItems { get; set; }

        private StationMenuItemModel _selectedMenuItem;
        public StationMenuItemModel SelectedMenuItem
        {
            get => _selectedMenuItem;
            set
            { 
                SetProperty(ref _selectedMenuItem, value);
                var targetView = _selectedMenuItem.Id switch
                {
                    1 => StationSiteHardwarePageViewModel.Tag,
                    2 => StationSiteControlChannelPageViewModel.Tag,
                    3 => StationSiteHydraulicPageViewModel.Tag,
                    4 => StationSitePseudoChannelPageViewModel.Tag,
                    5 => StationSiteModel3DSettingPageViewModel.Tag,
                    _ => "",
                };
                var paramters = new NavigationParameters { { "StationId", _stationId } };
                _regionManager.RequestNavigate(GlobalConstant.StationSiteNavigateRegionName, new Uri(targetView, UriKind.Relative), paramters);
            }
        }
        #endregion

        #region Command
        // public DelegateCommand ItemClickCommand => new(ExecuteItemClickCommand);
        public DelegateCommand LoadCommand => new(ExecuteLoadCommand); 
        #endregion

        #region Provate Method 
        private void ExecuteLoadCommand()
        {
            SelectedMenuItem = MenuItems[0]; // 默认选中第一个菜单项
        } 
        #endregion

    }
}
