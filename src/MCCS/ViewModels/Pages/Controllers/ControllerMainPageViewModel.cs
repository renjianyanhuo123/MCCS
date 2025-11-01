using System.Reactive.Linq;
using MCCS.Collecter.Services;
using MCCS.Common.DataManagers;
using MCCS.Core.Devices.Commands;
using MCCS.Events.Controllers;
using MCCS.Infrastructure.TestModels;
using MCCS.Infrastructure.TestModels.ControlParams;
using MCCS.Models;
using MCCS.ViewModels.Pages.ControlCommandPages;
using MCCS.Views.Pages.ControlCommandPages;
using MCCS.Components.GlobalNotification.Models;
using MCCS.Services.NotificationService;

namespace MCCS.ViewModels.Pages.Controllers
{
    public class ControllerMainPageViewModel : BindableBase
    {
        public const string Tag = "ControllerMainPage";
        private readonly IEventAggregator _eventAggregator;
        private readonly IControllerService _controllerService;
        private readonly INotificationService _notificationService;
        private readonly long _controllerId = -1;
        private readonly long _deviceId = -1;
        private IDisposable? _commandStatusSubscription;

        public ControllerMainPageViewModel(
            IControllerService controllerService,
            INotificationService notificationService,
            IEventAggregator eventAggregator)
        {
            _controllerService = controllerService;
            _eventAggregator = eventAggregator;
            _notificationService = notificationService;
        }

        public ControllerMainPageViewModel(
            long modelId,
            IControllerService controllerService,
            INotificationService notificationService,
            IEventAggregator eventAggregator) : this(controllerService, notificationService, eventAggregator)
        {
            IsParticipateControl = true;
            CurrentModelId = modelId;
            CurrentChannelName = "力/位移控制通道";
            ParticipateControlCommand = new DelegateCommand<long?>(ExecuteParticipateControlCommand);
            ControlModeSelectionChangedCommand = new DelegateCommand(ExecuteControlModeSelectionChangedCommand);
            ApplyCommand = new DelegateCommand(ExecuteApplyCommand);
            StopCommand = new DelegateCommand(ExecuteStopCommand);
            // 查找当前模型对应的设备ID  的  控制器ID
            var tempDevice = GlobalDataManager.Instance.Model3Ds?.FirstOrDefault(c => c.Id == modelId)?.MappingDevice;
            if (tempDevice?.ParentDeviceId== null) throw new ArgumentNullException("未配置连接到控制器");
            _controllerId = (long)tempDevice.ParentDeviceId;
            _deviceId = tempDevice.Id;
        }

        #region Proterty
        /// <summary>
        /// 选择的控制方式界面
        /// </summary>
        private System.Windows.Controls.UserControl _currentPage;
        public System.Windows.Controls.UserControl CurrentPage
        {
            get => _currentPage;
            set => SetProperty(ref _currentPage, value);
        }
        /// <summary>
        /// 当前指令的执行状态
        /// </summary>
        private CommandExecuteStatusEnum _currentCommandStatus;
        public CommandExecuteStatusEnum CurrentCommandStatus 
        {
            get => _currentCommandStatus;
            set => SetProperty(ref _currentCommandStatus, value);
        }
        /// <summary>
        /// 当前选择的控制模式
        /// </summary>
        private int _selectedControlMode = 0;
        public int SelectedControlMode
        {
            get => _selectedControlMode;
            set => SetProperty(ref _selectedControlMode, value);
        }
        /// <summary>
        /// 当前是否选中
        /// </summary>
        private bool _isSelected = false;
        public bool IsSelected
        {
            get => _isSelected;
            set => SetProperty(ref _isSelected, value);
        }
        /// <summary>
        /// 控制类型
        /// </summary>
        private ControlTypeEnum _controlType = ControlTypeEnum.Single;
        public ControlTypeEnum ControlType
        {
            get => _controlType;
            set => SetProperty(ref _controlType, value);
        }
        /// <summary>
        /// 当前模型Id
        /// </summary>
        private long _currentModelId = -1;
        public long CurrentModelId
        {
            get => _currentModelId;
            set => SetProperty(ref _currentModelId, value);
        }
        /// <summary>
        /// 当前通道名称
        /// </summary>
        private string _currentChannelName = string.Empty;
        public string CurrentChannelName 
        {
            get => _currentChannelName;
            set => SetProperty(ref _currentChannelName, value);
        }
        /// <summary>
        /// 是否参与控制
        /// </summary>
        private bool _isParticipateControl = false;
        public bool IsParticipateControl 
        {
            get => _isParticipateControl;
            set => SetProperty(ref _isParticipateControl, value);
        } 
        #endregion

        #region Command

        /// <summary>
        /// 是否参与控制
        /// </summary>
        public DelegateCommand<long?> ParticipateControlCommand { get; }

        /// <summary>
        /// 控制模式切换命令
        /// </summary>
        public DelegateCommand ControlModeSelectionChangedCommand { get; }

        /// <summary>
        /// 应用命令
        /// </summary>
        public DelegateCommand ApplyCommand { get; }
        /// <summary>
        /// 暂停命令
        /// </summary>
        public DelegateCommand StopCommand { get; }
        #endregion

        #region private method 
        /// <summary>
        /// 根据静态控制视图模型获取静态控制枚举
        /// </summary>
        /// <param name="viewModel"></param>
        /// <returns></returns>
        private static StaticLoadControlEnum GetStaticLoadControl(ViewStaticControlViewModel viewModel)
        {
            return viewModel switch
            {
                { SelectedControlUnitType: 0, SelectedTargetUnitType: 0 } => StaticLoadControlEnum.CTRLMODE_LoadS,
                { SelectedControlUnitType: 0, SelectedTargetUnitType: 1 } => StaticLoadControlEnum.CTRLMODE_LoadSVNP,
                { SelectedControlUnitType: 1, SelectedTargetUnitType: 0 } => StaticLoadControlEnum.CTRLMODE_LoadNVSP,
                { SelectedControlUnitType: 1, SelectedTargetUnitType: 1 } => StaticLoadControlEnum.CTRLMODE_LoadN,
                _ => StaticLoadControlEnum.CTRLMODE_LoadS
            };
        }


        /// <summary>
        /// 应用命令
        /// </summary>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        private void ExecuteApplyCommand()
        {
            if (_controllerId == -1 || _deviceId == -1) return;

            var controlMode = (ControlMode)SelectedControlMode;
            switch (controlMode)
            {
                case ControlMode.Fatigue:
                    if (CurrentPage.DataContext is ViewFatigueControlViewModel fatigueControlViewModel)
                    {
                        var success = _controllerService.DynamicControl(_controllerId, _deviceId, new DynamicControlParams
                        {
                            ControlMode = fatigueControlViewModel.ControlUnitType,
                            Amplitude = fatigueControlViewModel.Amplitude,
                            WaveType = fatigueControlViewModel.WaveformType,
                            Frequency = fatigueControlViewModel.Frequency,
                            MeanValue = fatigueControlViewModel.Median,
                            CompensateAmplitude = fatigueControlViewModel.CompensateAmplitude,
                            CompensationPhase = fatigueControlViewModel.CompensatePhase,
                            IsAdjustedMedian = false,
                            CycleCount = fatigueControlViewModel.CycleTimes
                        });
                        if (success)
                        {
                            _notificationService.Show("成功", "该通道成功启动疲劳控制!", NotificationType.Success);
                            SubscribeToCommandStatus();
                        }
                        else
                        {
                            _notificationService.Show("失败", "该通道启动疲劳控制失败!", NotificationType.Error);
                        }
                    }
                    break;
                case ControlMode.Static:
                    if (CurrentPage.DataContext is ViewStaticControlViewModel staticControlViewModel)
                    {
                        var success = _controllerService.StaticControl(_controllerId, _deviceId,
                            new StaticControlParams
                            {
                                StaticLoadControl = GetStaticLoadControl(staticControlViewModel),
                                Speed = staticControlViewModel.Speed,
                                TargetValue = staticControlViewModel.TargetValue
                            });
                        if (success)
                        {
                            _notificationService.Show("成功", "该通道成功启动静态控制！", NotificationType.Success);
                            SubscribeToCommandStatus();
                        }
                        else
                        {
                            _notificationService.Show("失败", "该通道启动静态控制失败!", NotificationType.Error);
                        }
                    }
                    break;
                case ControlMode.Programmable:
                    if (CurrentPage.DataContext is ViewProgramControlViewModel programControlViewModel)
                    {

                    }
                    break;
                case ControlMode.Manual:
                    if (CurrentPage.DataContext is ViewManualControlViewModel manualControlViewModel)
                    {
                    }
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// 订阅命令状态流
        /// </summary>
        private void SubscribeToCommandStatus()
        {
            // 取消之前的订阅
            _commandStatusSubscription?.Dispose();

            // 获取控制器并订阅命令状态流（只订阅当前设备的状态变化）
            var controller = _controllerService.GetControllerInfo(_controllerId);
            _commandStatusSubscription = controller
                .CommandStatusStream
                .Where(e => e.DeviceId == _deviceId)  // 过滤出当前设备的状态变化
                .ObserveOnDispatcher()
                .Subscribe(e => CurrentCommandStatus = e.Status);
        }

        /// <summary>
        /// 执行停止指令
        /// </summary>
        private void ExecuteStopCommand() 
        {
            //_eventAggregator.GetEvent<NotificationCommandStopedEvent>().Publish(new NotificationCommandStatusEventParam
            //{ 
            //    CommandId = CurrentChannelId, 
            //    CommandExecuteStatus = CommandExecuteStatusEnum.Stoping
            //});
        }

        private void SetView() 
        {
            var controlMode = (ControlMode)SelectedControlMode; 
            switch (controlMode)
            {
                case ControlMode.Fatigue: 
                    if (_controllerService.OperationControlMode(_controllerId, SystemControlState.Dynamic))
                    {
                        var fatigue = new ViewFatigueControl
                        {
                            DataContext = new ViewFatigueControlViewModel()
                        };
                        CurrentPage = fatigue;
                    } 
                    break;
                case ControlMode.Static: 
                    if (_controllerService.OperationControlMode(_controllerId, SystemControlState.Static))
                    {
                        var staticView = new ViewStaticControl
                        {
                            DataContext = new ViewStaticControlViewModel()
                        };
                        CurrentPage = staticView;
                    } 
                    break;
                case ControlMode.Programmable: 
                    if (_controllerService.OperationControlMode(_controllerId, SystemControlState.Static))
                    {
                        var programView = new ViewProgramControl
                        {
                            DataContext = new ViewProgramControlViewModel()
                        };
                        CurrentPage = programView;
                    } 
                    break;
                case ControlMode.Manual:
                    
                    if (_controllerService.OperationControlMode(_controllerId, SystemControlState.OpenLoop))
                    {
                        var manualView = new ViewManualControl
                        {
                            DataContext = new ViewManualControlViewModel()
                        };
                        CurrentPage = manualView;
                    }
                    break;
                default:
                    break;
            }  
        }
        private void ExecuteControlModeSelectionChangedCommand()
        {
            SetView();
        }
        /// <summary>
        /// 是否参与控制CheckBox
        /// </summary>
        /// <param name="modelId"></param>
        private void ExecuteParticipateControlCommand(long? modelId)
        {
            if (modelId == null) return;
            IsParticipateControl = false;
            _eventAggregator.GetEvent<InverseControlEvent>().Publish(new InverseControlEventParam
            {
                ModelId = (long)modelId
            });
        } 
        #endregion
    }
}
