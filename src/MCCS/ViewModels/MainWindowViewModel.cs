using MahApps.Metro.Controls;
using MaterialDesignThemes.Wpf;
using MCCS.Events;
using MCCS.ViewModels.Pages;
using System.Windows.Controls;

namespace MCCS.ViewModels
{
    public class MainWindowViewModel : BaseViewModel
    {
        private readonly IRegionManager _regionManager;

        #region 页面属性
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

        public DelegateCommand<object> HomePageLoadedCommand => new(ExcuateHomePageLoadedCommand);
        public DelegateCommand<object> JumpToCommand => new(ExecuteJumpToCommand);

        public DelegateCommand<Flyout> OpenTestFlyoutCommand => new(f => f!.SetCurrentValue(Flyout.IsOpenProperty, true), f => f is not null);
        #endregion
        private void ExcuateHomePageLoadedCommand(object parameter) 
        {
            var contentControl = parameter as ContentControl;
            string regionName = contentControl.GetValue(RegionManager.RegionNameProperty) as string;
            if (!string.IsNullOrEmpty(regionName))
            {
                if (_regionManager.Regions.ContainsRegionWithName(regionName) == false)
                {
                    RegionManager.SetRegionManager(contentControl, _regionManager);
                }
            }
        }
        private void ExecuteJumpToCommand(object parameter)
        {
            if (parameter is not HamburgerMenuIconItem) return;
            var item = parameter as HamburgerMenuIconItem ?? throw new NullReferenceException(nameof(HamburgerMenuIconItem));
            var i = _regionManager.Regions[GlobalConstant.MainContentRegionNam];
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

        public MainWindowViewModel(IRegionManager regionManager, IEventAggregator eventAggregator) : base(eventAggregator)
        {
            eventAggregator.GetEvent<OpenRightFlyoutEvent>().Subscribe(OnOpenRightFlyout);
            _regionManager = regionManager;
            _menus =
            [
                new HamburgerMenuIconItem()
                {
                    Icon = new PackIcon () { Kind = PackIconKind.Home },
                    Label = "主页",
                    Tag = HomePageViewModel.Tag
                },
                //new HamburgerMenuIconItem()
                //{
                //    Icon = new PackIconLucide () { Kind = PackIconLucideKind.Shield },
                //    Label = "保护",
                //    Tag = ViewMonitorViewModel.Tag
                //},
                //new HamburgerMenuIconItem()
                //{
                //    Icon = new PackIconFontAwesome () { Kind = PackIconFontAwesomeKind.DesktopSolid },
                //    Label = "监控",
                //    Tag = ViewMonitorViewModel.Tag
                //},
                //new HamburgerMenuIconItem()
                //{
                //    Icon = new PackIconFontAwesome() { Kind = PackIconFontAwesomeKind.DatabaseSolid },
                //    Label = "数据",
                //    Tag = ViewDataViewModel.Tag
                //},
                //new HamburgerMenuIconItem()
                //{
                //    Icon = new PackIconFontAwesome () { Kind = PackIconFontAwesomeKind.FlaskVialSolid },
                //    Label = "试验",
                //    Tag = ViewTestViewModel.Tag
                //}
            ];
            _optionsMenus = [
                //new HamburgerMenuIconItem()
                //{
                //    Icon = new PackIconCircumIcons() { Kind = PackIconCircumIconsKind.Settings},
                //    Label = "设置"
                //}
            ];
        }
    }
}
