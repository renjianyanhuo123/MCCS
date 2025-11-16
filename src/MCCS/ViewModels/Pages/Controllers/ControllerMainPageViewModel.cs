using MCCS.Collecter.ControlChannelManagers;
using MCCS.Collecter.ControllerManagers;
using MCCS.Common.DataManagers;
using MCCS.Events.Controllers;
using MCCS.Infrastructure.TestModels.ControlParams;
using MCCS.Models;
using MCCS.ViewModels.Pages.ControlCommandPages;
using MCCS.Views.Pages.ControlCommandPages;
using MCCS.Collecter.DllNative.Models;
using MCCS.Common.DataManagers.Model3Ds;
using MCCS.Components.GlobalNotification.Models;
using MCCS.Core.Models.StationSites;
using MCCS.Infrastructure.TestModels.Commands;
using MCCS.Models.ControlCommand;
using MCCS.Services.NotificationService;

namespace MCCS.ViewModels.Pages.Controllers
{
    public class ControllerMainPageViewModel : BindableBase
    {
        public const string Tag = "ControllerMainPage";  
        private readonly IEventAggregator _eventAggregator;
        private readonly IControlChannelManager _controlChannelManager;
        private readonly INotificationService _notificationService;
        private readonly IControllerManager _controllerManager;
        private readonly long _controllerId = -1;
        private readonly Model3DMainInfo _modelInfo;

        public ControllerMainPageViewModel(
            IControllerManager controllerManager,
            IControlChannelManager controlChannelManager,
            INotificationService notificationService,
            IEventAggregator eventAggregator)
        {
            _controlChannelManager = controlChannelManager;
            _eventAggregator = eventAggregator;
            _notificationService = notificationService;
            _controllerManager = controllerManager;
        }

        public ControllerMainPageViewModel(
            long modelId,
            IControllerManager controllerManager,
            IControlChannelManager controlChannelManager,
            INotificationService notificationService,
            IEventAggregator eventAggregator) : this(controllerManager, controlChannelManager, notificationService, eventAggregator)
        {
            IsParticipateControl = true;
            CurrentModelId = modelId;
            CurrentChannelName = "力/位移控制通道";
            CurrentCommandStatus = CommandExecuteStatusEnum.NoExecute; // 初始化为未执行状态，确保输入框可用
            ParticipateControlCommand = new DelegateCommand<long?>(ExecuteParticipateControlCommand);
            ControlModeSelectionChangedCommand = new DelegateCommand(ExecuteControlModeSelectionChangedCommand);
            ApplyCommand = new DelegateCommand(ExecuteApplyCommand);
            StopCommand = new DelegateCommand(ExecuteStopCommand);
            LoadCommand = new AsyncDelegateCommand(ExecuteLoadCommand);
            // 查找当前模型对应的设备ID  的  控制器ID
            _modelInfo = GlobalDataManager.Instance.Model3Ds?.FirstOrDefault(c => c.Id == modelId) ?? throw new ArgumentNullException("modeInfo is null");
            var tempDevice = _modelInfo?.MappingDevice;
            // var controlChannels = GlobalDataManager.Instance.Model3Ds
            if (tempDevice?.ParentDeviceId == null) throw new ArgumentNullException("未配置连接到控制器");
            _controllerId = (long)tempDevice.ParentDeviceId;
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

        public AsyncDelegateCommand LoadCommand { get; }

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
        private async Task ExecuteLoadCommand()
        {

        }

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
        /// 指令执行状态监听回调
        /// </summary>
        /// <param name="param"></param>
        private void OnCommandExecuteStatus(BatchCollectItemModel param)
        {
            //if ()
            //{

            //}
        }

        /// <summary>
        /// 应用命令
        /// </summary>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        private void ExecuteApplyCommand()
        {
            CurrentCommandStatus = CommandExecuteStatusEnum.Executing;
            var controlMode = (ControlMode)SelectedControlMode;
            bool commandSuccess = false;
            switch (controlMode)
            {
                case ControlMode.Fatigue:
                    if (CurrentPage.DataContext is ViewFatigueControlViewModel fatigueControlViewModel)
                    {
                        var selectedChannel = fatigueControlViewModel.SelectedChannel;
                        var commandContext = _controlChannelManager.DynamicControl(selectedChannel.ChannelId, new DynamicControlParams
                        {
                            ControlMode = fatigueControlViewModel.ControlUnitType,
                            Amplitude = fatigueControlViewModel.Amplitude,
                            WaveType = fatigueControlViewModel.WaveformType,
                            Frequency = fatigueControlViewModel.Frequency,
                            MeanValue = fatigueControlViewModel.Median,
                            CompensateAmplitude = fatigueControlViewModel.CompensateAmplitude,
                            CompensationPhase = fatigueControlViewModel.CompensatePhase,
                            CycleCount = fatigueControlViewModel.CycleTimes
                        });
                        if (commandContext.IsValid)
                        {
                            _notificationService.Show("成功", "该通道成功启动疲劳控制!", NotificationType.Success);
                            commandSuccess = true;
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
                        var selectedChannel = staticControlViewModel.SelectedChannel;
                        var commandContext = _controlChannelManager.StaticControl(selectedChannel.ChannelId,
                            new StaticControlParams
                            {
                                StaticLoadControl = GetStaticLoadControl(staticControlViewModel),
                                Speed = staticControlViewModel.Speed,
                                TargetValue = staticControlViewModel.TargetValue
                            });
                        if (commandContext.IsValid)
                        {
                            _notificationService.Show("成功", "该通道成功启动静态控制！", NotificationType.Success);
                            commandSuccess = true;
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
                        var controlChannel = _modelInfo.ControlChannelInfos.FirstOrDefault(c => c.ChannelType is ChannelTypeEnum.Position or ChannelTypeEnum.Mix);
                        if (controlChannel == null)
                        {
                            _notificationService.Show("失败", "该通道启动手动控制失败!", NotificationType.Error);
                            break;
                        }
                        var commandContext = _controlChannelManager.ManualControl(controlChannel.Id, (float)manualControlViewModel.OutPutValue);
                        if (commandContext.IsValid)
                        {
                            _notificationService.Show("成功", "该通道成功启动手动控制！", NotificationType.Success);
                            commandSuccess = true;
                        }
                        else
                        {
                            _notificationService.Show("失败", "该通道启动手动控制失败!", NotificationType.Error);
                        }
                    }
                    break;
                default:
                    break;
            }

            // 命令执行失败时，恢复为NoExecute状态，允许用户重新输入参数
            if (!commandSuccess)
            {
                CurrentCommandStatus = CommandExecuteStatusEnum.NoExecute;
            }
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

            // 停止后恢复为NoExecute状态，允许用户重新输入参数
            CurrentCommandStatus = CommandExecuteStatusEnum.NoExecute;
        }

        private void SetView() 
        {
            var controlMode = (ControlMode)SelectedControlMode;
            var controlChannels = _modelInfo.ControlChannelInfos.Select(s => new ControlChannelBindModel
            {
                ChannelId = s.Id,
                ChannelName = s.Name,
                ChannelType = s.ChannelType
            });
            switch (controlMode)
            {
                case ControlMode.Fatigue: 
                    var fatigue = new ViewFatigueControl
                    {
                        DataContext = new ViewFatigueControlViewModel(controlChannels)
                    };
                    CurrentPage = fatigue;
                    break;
                case ControlMode.Static:  
                    var staticView = new ViewStaticControl
                    {
                        DataContext = new ViewStaticControlViewModel(controlChannels)
                    };
                    CurrentPage = staticView;
                    break;
                case ControlMode.Programmable:  
                    var programView = new ViewProgramControl
                    {
                        DataContext = new ViewProgramControlViewModel(controlChannels)
                    };
                    CurrentPage = programView;
                    break;
                case ControlMode.Manual: 
                    var manualView = new ViewManualControl
                    {
                        DataContext = new ViewManualControlViewModel()
                    };
                    CurrentPage = manualView;
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
