using MahApps.Metro.Controls;
using MCCS.Core.Repositories;
using MCCS.Events;
using MCCS.ViewModels.Others;
using MCCS.ViewModels.Pages;
using System.Collections.ObjectModel;
using System.Windows;
using MCCS.Common;
using System.Diagnostics;
using MCCS.Views;

namespace MCCS.ViewModels
{
    public class MainWindowViewModel : BaseViewModel
    {
        private readonly IRegionManager _regionManager;
        private readonly ISystemMenuRepository _systemMenuRepository;
        private readonly IContainerProvider _containerProvider;

        #region 页面属性
        private ObservableCollection<MainTabsViewModel> _tabs;
        public ObservableCollection<MainTabsViewModel> Tabs { 
            get => _tabs; 
            set => SetProperty(ref _tabs, value);
        }

        private HamburgerMenuItem _selectedMenuItem;
        public HamburgerMenuItem SelectedMenuItem
        {
            get => _selectedMenuItem;
            set => SetProperty(ref _selectedMenuItem, value);
        }
        private MainTabsViewModel _selectedTabItem;
        public MainTabsViewModel SelectedTabItem 
        {
            get => _selectedTabItem;
            set
            {
                if (value == _selectedTabItem) return;
                foreach (var tab in _tabs) tab.IsChecked = tab.Id == value.Id;
                _regionManager.RequestNavigate(GlobalConstant.MainContentRegionName, new Uri(value.Id, UriKind.Relative));
                SetProperty(ref _selectedTabItem, value);
            }
        }

        private List<HamburgerMenuIconItem> _menus;
        public List<HamburgerMenuIconItem> Menus
        {
            get => _menus;
            set => SetProperty(ref _menus, value);
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
        public DelegateCommand<object> MainRegionLoadedCommand => new(ExecuteMainRegionLoadedCommand);
        public DelegateCommand<object> JumpChildTabCommand => new(ExecuteJumpChildTabCommand);
        public DelegateCommand<object> JumpToCommand => new(ExecuteJumpToCommand);
        public DelegateCommand<string> CloseTabCommand => new(ExecuteCloseTabCommand); 
        #endregion

        #region 命令执行
        private void ExecuteCloseTabCommand(string id) 
        {
            var delItem = _tabs.First(x => x.Id == id);
            if (SelectedTabItem.Id == id)
            {
                // 默认跳转首页
                SelectedTabItem = _tabs.First(c => c.Id == HomePageViewModel.Tag);
            }
            _tabs.Remove(delItem); 
        }

        private void ExecuteMainRegionLoadedCommand(object parameter)
        {
            _regionManager.RequestNavigate(GlobalConstant.MainContentRegionName, new Uri(SelectedMenuItem.Tag?.ToString() ?? "", UriKind.Relative));
        }
        private void ExecuteJumpChildTabCommand(object param)
        {
            SelectedTabItem = _tabs.First(c => c.Id == param.ToString());
        }
        private void ExecuteJumpToCommand(object parameter)
        {
            if (parameter is not HamburgerMenuIconItem iconItem) return;
            var item = iconItem ?? throw new NullReferenceException(nameof(HamburgerMenuIconItem));
            _regionManager.RequestNavigate(GlobalConstant.MainContentRegionName, new Uri(item.Tag?.ToString() ?? "", UriKind.Relative));
        }
        private void OnOpenTabPage(OpenTestOperationEventParam param) 
        {
            var addItem = _tabs.FirstOrDefault(c => c.Id == param.TabId);
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
            IContainerProvider containerProvider,
            ISystemMenuRepository systemMenuRepository,
            IRegionManager regionManager, 
            IEventAggregator eventAggregator, HamburgerMenuItem selectedMenuItem, MainTabsViewModel selectedTabItem) : base(eventAggregator)
        {
            eventAggregator.GetEvent<OpenTestOperationEvent>().Subscribe(OnOpenTabPage);
            _containerProvider = containerProvider;
            _systemMenuRepository = systemMenuRepository;
            _regionManager = regionManager;
            _selectedMenuItem = selectedMenuItem;
            _selectedTabItem = selectedTabItem;
            var menus = systemMenuRepository.GetChildMenusById(0);
            _menus = menus.Select(s => new HamburgerMenuIconItem
            {
                Icon = StringToIcon.ConvertToIcon(s.Icon),
                Label = s.Name,
                Tag = s.Key
            }).ToList();
            _tabs = [
                new MainTabsViewModel
                {
                    Id = HomePageViewModel.Tag,
                    Content = "主页",
                    IsEnable = true,
                    IsChecked = true,
                    IsEnableClose = Visibility.Collapsed,
                }
            ];
            // 默认选中第一个项，并执行导航
            SelectedTabItem = _tabs.FirstOrDefault() ?? throw new ArgumentNullException(nameof(_tabs));
            SelectedMenuItem = _menus[1]; 
            _optionsMenus = [];
        }
    }
}
