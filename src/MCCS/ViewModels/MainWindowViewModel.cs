using MahApps.Metro.Controls;
using MaterialDesignThemes.Wpf;
using MCCS.Core.Repositories;
using MCCS.Events;
using MCCS.ViewModels.Others;
using MCCS.ViewModels.Pages;
using System.Collections.ObjectModel;
using System.Windows;

namespace MCCS.ViewModels
{
    public class MainWindowViewModel : BaseViewModel
    {
        private readonly IRegionManager _regionManager;
        private readonly ISystemMenuRepository _systemMenuRepository;

        #region 页面属性
        private ObservableCollection<MainTabsViewModel> _tabs;
        public ObservableCollection<MainTabsViewModel> Tabs { get => _tabs; set { SetProperty(ref _tabs, value); } }

        private HamburgerMenuItem _selectedMenuItem;
        public HamburgerMenuItem SelectedMenuItem
        {
            get => _selectedMenuItem;
            set 
            {
                SetProperty(ref _selectedMenuItem, value);
            }
        }
        private MainTabsViewModel _selectedTabItem;
        public MainTabsViewModel SelectedTabItem 
        {
            get => _selectedTabItem;
            set 
            {
                if (value != _selectedTabItem) 
                {
                    foreach (var tab in _tabs) tab.IsChecked = tab.Id == value.Id;
                    _regionManager.RequestNavigate(GlobalConstant.MainContentRegionNam, new Uri(value.Id, UriKind.Relative), NavigationCompleted);
                    SetProperty(ref _selectedTabItem, value);
                }
            }
        }

        private List<HamburgerMenuIconItem> _menus;
        public List<HamburgerMenuIconItem> Menus
        {
            get => _menus;
            set => SetProperty(ref _menus, value);
        }

        private bool _isOpenFlyout = false;
        public bool IsOpenFlyout
        {
            get => _isOpenFlyout;
            set => SetProperty(ref _isOpenFlyout, value);
        }

        private List<HamburgerMenuIconItem> _optionsMenus;
        public List<HamburgerMenuIconItem> OptionsMenus
        {
            get => _optionsMenus;
            set => SetProperty(ref _optionsMenus, value);
        }

        private string _rightFlyoutName = string.Empty;
        public string RightFlyoutName
        {
            get => _rightFlyoutName;
            set => SetProperty(ref _rightFlyoutName, value);
        }
        #endregion

        #region 命令声明
        public DelegateCommand<object> MainRegionLoadedCommand => new(ExcuateMainRegionLoadedCommand);
        public DelegateCommand<object> JumpChildTabCommand => new(ExcuateJumpChildTabCommand);
        public DelegateCommand<object> JumpToCommand => new(ExecuteJumpToCommand);
        public DelegateCommand<Flyout> OpenTestFlyoutCommand => new(f => f!.SetCurrentValue(Flyout.IsOpenProperty, true), f => f is not null);
        public DelegateCommand<string> CloseTabCommand => new(ExcuateCloseTabCommand);
        #endregion

        #region 命令执行
        private void ExcuateCloseTabCommand(string id) 
        {
            var delItem = _tabs.First(x => x.Id == id);
            if (SelectedTabItem.Id == id)
            {
                // 默认跳转首页
                SelectedTabItem = _tabs.First(c => c.Id == HomePageViewModel.Tag);
            }
            _tabs.Remove(delItem); 
        }

        private void ExcuateMainRegionLoadedCommand(object parameter)
        {
            _regionManager.RequestNavigate(GlobalConstant.MainContentRegionNam, new Uri(SelectedMenuItem.Tag?.ToString() ?? "", UriKind.Relative), NavigationCompleted);
        }
        private void ExcuateJumpChildTabCommand(object param)
        {
            SelectedTabItem = _tabs.First(c => c.Id == param.ToString());
        }
        private void ExecuteJumpToCommand(object parameter)
        {
            if (parameter is not HamburgerMenuIconItem) return;
            var item = parameter as HamburgerMenuIconItem ?? throw new NullReferenceException(nameof(HamburgerMenuIconItem));
            _regionManager.RequestNavigate(GlobalConstant.MainContentRegionNam, new Uri(item.Tag?.ToString() ?? "", UriKind.Relative), NavigationCompleted);
        }

        private void NavigationCompleted(NavigationResult result)
        {
        }

        /// <summary>
        /// 打开右侧Modal
        /// </summary>
        /// <param name="param"></param>
        private void OnOpenRightFlyout(OpenRightFlyoutEventParam param)
        {
            IsOpenFlyout = true;
            switch (param.Type)
            {
                //case RightFlyoutTypeEnum.ControlCommand:
                //    var header = param.Others as TestControlCommandParam ?? throw new ArgumentNullException(nameof(TestControlCommandParam));
                //    RightFlyoutName = $"控制面{header.Face}第{header.Row}行;第{header.Column}列";
                //    _regionManager.RequestNavigate(GlobalConstant.RightFlyoutRegionName, new Uri(ViewTestCommandDialogViewModel.Tag, UriKind.Relative), NavigationCompleted);
                //    break;
                //default:
                //    break;
            }
        }

        private void OnOpenTabPage(OpenTestOperationEventParam param) 
        {
            MainTabsViewModel? addItem = _tabs.FirstOrDefault(c => c.Id == param.TabId);
            if (addItem == null) 
            {
                addItem = new MainTabsViewModel
                {
                    Id = param.TabId,
                    Content = param.TestName,
                    IsEnableClose = Visibility.Visible,
                    IsEnable = true
                };
                _tabs.Add(addItem);
            }
            SelectedTabItem = addItem;
        }
        #endregion



        public MainWindowViewModel(
            ISystemMenuRepository systemMenuRepository,
            IRegionManager regionManager, 
            IEventAggregator eventAggregator) : base(eventAggregator)
        {
            eventAggregator.GetEvent<OpenRightFlyoutEvent>().Subscribe(OnOpenRightFlyout);
            eventAggregator.GetEvent<OpenTestOperationEvent>().Subscribe(OnOpenTabPage);
            _regionManager = regionManager;
            var menus = systemMenuRepository.GetChildMenusById(0);
            _menus = menus.Select(s =>
            {
                if (!Enum.TryParse(s.Icon, out PackIconKind result))
                {
                    result = PackIconKind.Home;
                }
                return new HamburgerMenuIconItem
                {
                    Icon = new PackIcon() { Kind = result },
                    Label = s.Name,
                    Tag = s.Key
                };
            }).ToList();
            _tabs = [
                new()
                {
                    Id = HomePageViewModel.Tag,
                    Content = "主页",
                    IsEnable = true,
                    IsChecked = true,
                    IsEnableClose = Visibility.Collapsed,
                }
            ];
            // 默认选中第一个项，并执行导航
            SelectedTabItem = _tabs.FirstOrDefault();
            SelectedMenuItem = _menus.FirstOrDefault(); 
            _optionsMenus = [];
        }
    }
}
