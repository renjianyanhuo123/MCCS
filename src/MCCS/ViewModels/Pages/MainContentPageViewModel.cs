using System.Collections.ObjectModel;
using System.Windows;

using MahApps.Metro.Controls;

using MCCS.Common;
using MCCS.Common.Resources.Extensions;
using MCCS.Common.Resources.ViewModels;
using MCCS.Events;
using MCCS.Infrastructure.Repositories;
using MCCS.Models.MainPages;
using MCCS.Services.AppExitService;
using MCCS.ViewModels.Others;

namespace MCCS.ViewModels.Pages
{
    public class MainContentPageViewModel : BaseViewModel
    {
        public const string Tag = "MainContentPage";

        private readonly IRegionManager _regionManager;
        private readonly ISystemMenuRepository _systemMenuRepository; 
        private readonly IAppExitService _appExitService;

        #region 页面属性
        private ObservableCollection<MainTabsViewModel> _tabs;
        public ObservableCollection<MainTabsViewModel> Tabs
        {
            get => _tabs;
            set => SetProperty(ref _tabs, value);
        }

        private MainMenuItemModel? _selectedMenuItem;
        public MainMenuItemModel? SelectedMenuItem
        {
            get => _selectedMenuItem;
            set 
            {
                if (SetProperty(ref _selectedMenuItem, value)) 
                    _regionManager.RequestNavigate(GlobalConstant.MainContentRegionName, new Uri(_selectedMenuItem?.Key ?? "", UriKind.Relative));
            }
        }
        private MainTabsViewModel? _selectedTabItem;
        public MainTabsViewModel? SelectedTabItem
        {
            get => _selectedTabItem;
            set
            {
                if (value == _selectedTabItem || value == null) return;
                foreach (var tab in _tabs) tab.IsChecked = tab.Id == value?.Id; 
                SetProperty(ref _selectedTabItem, value);
            }
        }

        private List<MainMenuItemModel> _menus;
        public List<MainMenuItemModel> Menus
        {
            get => _menus;
            set => SetProperty(ref _menus, value);
        }

        private List<MainMenuItemModel> _optionsMenus;
        public List<MainMenuItemModel> OptionsMenus
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

        public AsyncDelegateCommand LogoutCommand { get; }

        #endregion

        #region 命令执行
        private void ExecuteCloseTabCommand(string id)
        {
            var delItem = _tabs.First(x => x.Id == id);
            if (SelectedTabItem?.Id == id)
            {
                // 默认跳转首页
                SelectedTabItem = _tabs.First(c => c.Id == HomePageViewModel.Tag);
            }
            _tabs.Remove(delItem);
        }

        private async Task ExecuteLogoutCommand()
        {
            var parameters = new DialogParameters
            {
                {"Title", "关闭应用程序"},
                { "ShowContent", "是否确定关闭整个应用程序?"},
                {"RootDialogName", "RootDialog"}
            }; 
            var result = await _dialogService.ShowDialogHostAsync(nameof(DeleteConfirmDialogViewModel), "RootDialog", parameters);
            if (result.Result == ButtonResult.OK)
            {
                await _appExitService.ExitAsync();
            }
        }

        private void ExecuteMainRegionLoadedCommand(object parameter) => _regionManager.RequestNavigate(GlobalConstant.MainContentRegionName, new Uri(SelectedMenuItem?.Key ?? "", UriKind.Relative));
        private void ExecuteJumpChildTabCommand(object param) => SelectedTabItem = _tabs.First(c => c.Id == param.ToString());

        private void ExecuteJumpToCommand(object parameter)
        {
            if (parameter is not HamburgerMenuIconItem iconItem) return;
            var item = iconItem ?? throw new NullReferenceException(nameof(HamburgerMenuIconItem));
            _regionManager.RequestNavigate(GlobalConstant.MainContentRegionName, new Uri(item.Tag?.ToString() ?? "", UriKind.Relative));
        }
        private void OnCancelSelectedMenu(NotificationCancelSelectedEventParam param) => SelectedMenuItem = null!;

        #endregion
         
        public MainContentPageViewModel( 
            ISystemMenuRepository systemMenuRepository,
            IRegionManager regionManager,
            IAppExitService appExitService,
            IEventAggregator eventAggregator,
            IDialogService dialogService) : base(eventAggregator, dialogService)
        {
            _eventAggregator.GetEvent<NotificationCancelSelectedEvent>().Subscribe(OnCancelSelectedMenu); 
            _systemMenuRepository = systemMenuRepository;
            _regionManager = regionManager;
            _appExitService = appExitService;
            LogoutCommand = new AsyncDelegateCommand(ExecuteLogoutCommand);
            var menus = systemMenuRepository.GetChildMenusById(0);
            _menus = [.. menus.Select(s => new MainMenuItemModel
            {
                SelectedIcon = StringToIcon.ConvertToIcon(s.Icon),
                UnselectedIcon = StringToIcon.ConvertToIcon(s.Icon),
                Name = s.Name,
                Key = s.Key
            })];
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
            SelectedMenuItem = _menus[0];
            _optionsMenus = [];
        }
    }
}
