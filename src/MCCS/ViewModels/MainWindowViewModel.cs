using System.Windows;

using MCCS.Common.Resources.ViewModels;
using MCCS.Events.Common;
using MCCS.Events.Mehtod.DynamicGridOperationEvents;
using MCCS.Events.StartUp;
using MCCS.ViewModels.MethodManager;
using MCCS.ViewModels.Pages;
using MCCS.Workflow.Contact.Events;
using MCCS.Workflow.StepComponents.ViewModels;
using MCCS.WorkflowSetting.EventParams;

namespace MCCS.ViewModels
{
    public class MainWindowViewModel : BaseViewModel
    {
        private readonly IRegionManager _regionManager;
        private string _currentFlyoutName = "";
        private object? _tempParam = null;

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
            set
            {
                if (SetProperty(ref _isOpenFlyout, value) && _isOpenFlyout == false)
                {
                    if (_currentFlyoutName == WorkflowStepListPageViewModel.Tag && _tempParam is AddOpEventParam param)
                    {
                        _eventAggregator.GetEvent<AddNodeEvent>().Publish(new AddNodeEventParam
                        {
                            Source = param.Source.ToString() ?? "",
                            Node = null
                        });
                    }
                }
            }
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

        // public DelegateCommand<Flyout> OpenTestFlyoutCommand { get; }
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
                // 注意: 这里的顺序不能乱
                WindowState = WindowState.Maximized; 
                _regionManager.RequestNavigate(GlobalConstant.StartUpRegionName, new Uri(MainContentPageViewModel.Tag, UriKind.Relative));
            }
        }

        /// <summary>
        /// 打开右侧Modal
        /// </summary>
        /// <param name="param"></param>
        private void OnOpenRightFlyout(OpenRightFlyoutEventParam param)
        {
            
        }

        private void ExecuteShowStepsCommand(AddOpEventParam opEventArgs)
        {
             IsOpenFlyout = true; 
            RightFlyoutName = "工作流配置";
            _currentFlyoutName = WorkflowStepListPageViewModel.Tag;
            _tempParam = opEventArgs;
            var paramters = new NavigationParameters { { "OpEventArgs", opEventArgs } }; 
             _regionManager.RequestNavigate(GlobalConstant.RightFlyoutRegionName, new Uri(WorkflowStepListPageViewModel.Tag, UriKind.Relative), paramters);
        }

        private void ExecuteShowComponentsCommand(OpenUiCompontsEventParam param)
        {
            IsOpenFlyout = true;
            RightFlyoutName = "Ui节点配置";
            _currentFlyoutName = nameof(MethodComponentsPageViewModel);
            _tempParam = param;
            var paramters = new NavigationParameters { { "SourceId", param.SourceId } };
            _regionManager.RequestNavigate(GlobalConstant.RightFlyoutRegionName, new Uri(nameof(MethodComponentsPageViewModel), UriKind.Relative), paramters);
        }

        private void ExecuteShowParamterSetCommand(OpenParamterSetEventParam param)
        {
            IsOpenFlyout = true;
            RightFlyoutName = "节点参数配置";
            _currentFlyoutName = param.ViewName;
            _tempParam = param;
            var paramters = new NavigationParameters { { "OpenParameterSetEventParam", param } };
            _regionManager.RequestNavigate(GlobalConstant.RightFlyoutRegionName, new Uri(param.ViewName, UriKind.Relative), paramters);
        }
        #endregion

        public MainWindowViewModel( 
            IRegionManager regionManager, 
            IEventAggregator eventAggregator) : base(eventAggregator)
        {
            ShowTitleBar = false;
            ShowCloseButton = false;
            _regionManager = regionManager; 
            _eventAggregator.GetEvent<FinishStartUpNotificationEvent>().Subscribe(JumpToMainPage);
            _eventAggregator.GetEvent<OpenRightFlyoutEvent>().Subscribe(OnOpenRightFlyout);
            _eventAggregator.GetEvent<AddOpEvent>().Subscribe(ExecuteShowStepsCommand);
            _eventAggregator.GetEvent<OpenUiCompontsEvent>().Subscribe(ExecuteShowComponentsCommand);
            _eventAggregator.GetEvent<OpenParamterSetEvent>().Subscribe(ExecuteShowParamterSetCommand);
            _eventAggregator.GetEvent<SelectedComponentEvent>().Subscribe(_ => IsOpenFlyout = false, ThreadOption.UIThread, false, filter => !string.IsNullOrEmpty(filter.SourceId));
            _eventAggregator.GetEvent<AddNodeEvent>().Subscribe(_ =>
            {
                _currentFlyoutName = "";
                IsOpenFlyout = false;
            }, ThreadOption.UIThread, false, filter => filter.Node != null);
            _eventAggregator.GetEvent<SaveParameterEvent>().Subscribe(_ => IsOpenFlyout = false, ThreadOption.UIThread, false, filter => !string.IsNullOrEmpty(filter.SourceId));
            LoadCommand = new DelegateCommand(ExecuteLoadCommand);
            // OpenTestFlyoutCommand = new DelegateCommand<Flyout>(f => f.SetCurrentValue(Flyout.IsOpenProperty, true), f => true);
        } 
    }
}
