using System.Windows;
using MCCS.Core.Repositories;
using MCCS.Events.StartUp;
using MCCS.ViewModels.Pages;

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

        #endregion

        #region Command 
        public DelegateCommand LoadCommand => new(ExecuteLoadCommand);
        #endregion

        #region PrivateMethod 
        private void ExecuteLoadCommand()
        { 
            ShowTitleBar = false;
            ShowCloseButton = false; 
            _regionManager.RequestNavigate(GlobalConstant.StartUpRegionName, new Uri(SplashPageViewModel.Tag, UriKind.Relative));
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
        }

        private void JumpToMainPage(FinishStartUpNotificationEventParam param)
        {
            if (param.IsSuccess)
            {
                ShowTitleBar = true;
                WindowState = WindowState.Maximized;
                ShowCloseButton = true; 
                _regionManager.RequestNavigate(GlobalConstant.StartUpRegionName, new Uri(MainContentPageViewModel.Tag, UriKind.Relative));
            }
        }
    }
}
