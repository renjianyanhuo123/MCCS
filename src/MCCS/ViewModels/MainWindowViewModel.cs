using System.Windows;
using MahApps.Metro.Controls;
using MCCS.Core.Repositories;
using MCCS.Events.Common;
using MCCS.Events.StartUp;
using MCCS.ViewModels.Pages;
using MCCS.ViewModels.Pages.WorkflowSteps;
using Prism.Navigation.Regions;

namespace MCCS.ViewModels
{
    public class MainWindowViewModel : BaseViewModel
    {
        private readonly IRegionManager _regionManager;
        private readonly ISystemMenuRepository _systemMenuRepository;
        private readonly IContainerProvider _containerProvider; 

        #region 页面属性 
        private double _mainPageWidth; 
        public double MainPageWidth
        {
            get => _mainPageWidth;
            set => SetProperty(ref _mainPageWidth, value);
        }

        private double _mainPageHeight;
        public double MainPageHeight
        {
            get => _mainPageHeight;
            set => SetProperty(ref _mainPageHeight, value);
        }

        private WindowStyle _windowStyle; 
        public WindowStyle WindowStyle
        {
            get => _windowStyle;
            set => SetProperty(ref _windowStyle, value);
        }

        private WindowState _windowState;
        public WindowState WindowState
        {
            get => _windowState;
            set => SetProperty(ref _windowState, value);
        }

        private ResizeMode _resizeMode; 
        public ResizeMode ResizeMode
        {
            get => _resizeMode;
            set => SetProperty(ref _resizeMode, value);
        }

        private bool _showTitleBar = false; 
        public bool ShowTitleBar
        {
            get => _showTitleBar;
            set => SetProperty(ref _showTitleBar, value);
        }

        private bool _showCloseButton = false; 
        public bool ShowCloseButton
        {
            get => _showCloseButton;
            set => SetProperty(ref _showCloseButton, value);
        }

        private bool _isOpenFlyout = false;
        public bool IsOpenFlyout
        {
            get => _isOpenFlyout;
            set => SetProperty(ref _isOpenFlyout, value);
        }

        private string _rightFlyoutName = string.Empty;
        public string RightFlyoutName
        {
            get => _rightFlyoutName;
            set => SetProperty(ref _rightFlyoutName, value);
        }
        #endregion

        #region Command 
        public DelegateCommand LoadCommand { get; }

        public DelegateCommand<Flyout> OpenTestFlyoutCommand { get; }
        #endregion

        #region PrivateMethod 
        private void ExecuteLoadCommand()
        { 
            ShowTitleBar = false;
            ShowCloseButton = false; 
            _regionManager.RequestNavigate(GlobalConstant.StartUpRegionName, new Uri(SplashPageViewModel.Tag, UriKind.Relative));
        }
        private void JumpToMainPage(FinishStartUpNotificationEventParam param)
        {
            if (param.IsSuccess)
            {
                ShowTitleBar = true;
                // 注意: 这里的顺序不能乱
                WindowState = WindowState.Maximized;
                ShowCloseButton = true;
                _regionManager.RequestNavigate(GlobalConstant.StartUpRegionName, new Uri(MainContentPageViewModel.Tag, UriKind.Relative));
            }
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
                case RightFlyoutTypeEnum.WorkflowSetting: 
                    RightFlyoutName = "工作流配置";
                    _regionManager.RequestNavigate(GlobalConstant.RightFlyoutRegionName, new Uri(WorkflowStepListPageViewModel.Tag, UriKind.Relative));
                    break;
                default:
                    break;
            }
        }
        #endregion

        public MainWindowViewModel(
            IContainerProvider containerProvider, 
            IRegionManager regionManager, 
            IEventAggregator eventAggregator) : base(eventAggregator)
        {
            ShowTitleBar = false;
            ShowCloseButton = false;
            _regionManager = regionManager; 
            _eventAggregator.GetEvent<FinishStartUpNotificationEvent>().Subscribe(JumpToMainPage);
            _eventAggregator.GetEvent<OpenRightFlyoutEvent>().Subscribe(OnOpenRightFlyout);
            LoadCommand = new DelegateCommand(ExecuteLoadCommand);
            OpenTestFlyoutCommand = new DelegateCommand<Flyout>(f => f.SetCurrentValue(Flyout.IsOpenProperty, true), f => true);
        } 
    }
}
