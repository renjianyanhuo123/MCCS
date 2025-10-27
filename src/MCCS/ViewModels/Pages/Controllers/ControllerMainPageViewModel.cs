using MCCS.Collecter.Services;
using MCCS.Core.Devices.Commands; 
using MCCS.Events.Controllers;
using MCCS.Models;
using MCCS.ViewModels.Pages.ControlCommandPages;
using MCCS.Views.Pages.ControlCommandPages;

namespace MCCS.ViewModels.Pages.Controllers
{
    public class ControllerMainPageViewModel : BindableBase
    {
        public const string Tag = "ControllerMainPage";  
        private readonly IEventAggregator _eventAggregator;
        private readonly IControllerService _controllerService;

        private bool _isShowController = false;
        private bool _isParticipateControl = false; 

        private bool _isSelected = false;  
        private long _currentModelId = -1;

        private ControlTypeEnum _controlType = ControlTypeEnum.Single; 
        private int _selectedControlMode = 0; 
        private string _currentChannelName = string.Empty; 

        private CommandExecuteStatusEnum _currentCommandStatus;

        public ControllerMainPageViewModel(
            IControllerService controllerService,
            IEventAggregator eventAggregator)
        {
            _controllerService = controllerService;
            _eventAggregator = eventAggregator;  
        }

        public ControllerMainPageViewModel(
            long modelId,
            IControllerService controllerService,
            IEventAggregator eventAggregator) : this(controllerService, eventAggregator)
        {
            IsParticipateControl = true;
            CurrentModelId = modelId;
            CurrentChannelName = "力/位移控制通道";
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
        public CommandExecuteStatusEnum CurrentCommandStatus 
        {
            get => _currentCommandStatus;
            set => SetProperty(ref _currentCommandStatus, value);
        }
        /// <summary>
        /// 当前选择的控制模式
        /// </summary>
        public int SelectedControlMode
        {
            get => _selectedControlMode;
            set => SetProperty(ref _selectedControlMode, value);
        } 
        /// <summary>
        /// 当前是否选中
        /// </summary>
        public bool IsSelected
        {
            get => _isSelected;
            set => SetProperty(ref _isSelected, value);
        }  
        /// <summary>
        /// 控制类型
        /// </summary>
        public ControlTypeEnum ControlType
        {
            get => _controlType;
            set => SetProperty(ref _controlType, value);
        }
        /// <summary>
        /// 当前模型Id
        /// </summary>
        public long CurrentModelId
        {
            get => _currentModelId;
            set => SetProperty(ref _currentModelId, value);
        }
        /// <summary>
        /// 当前通道名称
        /// </summary>
        public string CurrentChannelName 
        {
            get => _currentChannelName;
            set => SetProperty(ref _currentChannelName, value);
        } 
        /// <summary>
        /// 是否参与控制
        /// </summary>
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
        public DelegateCommand<long?> ParticipateControlCommand => new(ExecuteParticipateControlCommand);
        /// <summary>
        /// 控制模式切换命令
        /// </summary>
        public DelegateCommand ControlModeSelectionChangedCommand => new(ExecuteControlModeSelectionChangedCommand);
        /// <summary>
        /// 应用命令
        /// </summary>
        public DelegateCommand ApplyCommand => new(ExecuteApplyCommand); 

        public DelegateCommand StopCommand => new(ExecuteStopCommand);
        #endregion 

        #region private method 
        /// <summary>
        /// 应用命令
        /// </summary>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        private void ExecuteApplyCommand() 
        {
            CurrentCommandStatus = CommandExecuteStatusEnum.Executing;
            //var model = _controlInfoDic[CurrentChannelId].ControlParams;
            //var res = new ReceivedCommandDataEventParam
            //{
            //    ControlMode = (ControlMode)SelectedControlMode,
            //    Param = _tempSaveControlParamInfo
            //};
            //if (ControlType == ControlTypeEnum.Single)
            //{
            //    res.ChannelIds.Add(CurrentChannelId);
            //}
            //else
            //{
            //    var combineInfo = _controlCombineInfos.FirstOrDefault(c => c.CombineChannelId == SelectedControlCombineInfo.CombineChannelId);
            //    if (combineInfo != null)
            //    {
            //        res.ChannelIds.AddRange(combineInfo.ControlChannels.Select(s => s.ChannelId));
            //    }
            //}
            //_eventAggregator.GetEvent<ReceivedCommandDataEvent>().Publish(res);
            //_eventAggregator.GetEvent<NotificationCommandFinishedEvent>().Subscribe(res =>
            //{
            //    var success = _controlInfoDic.TryGetValue(res.CommandId, out var controlInfo);
            //    if (!success || controlInfo == null) return;
            //    controlInfo.ExecutingStatus = res.CommandExecuteStatus;
            //    if (CurrentChannelId == res.CommandId) CurrentCommandStatus = res.CommandExecuteStatus;
            //});
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
                    var fatigue = new ViewFatigueControl
                    {
                        DataContext = new ViewFatigueControlViewModel()
                    };
                    CurrentPage = fatigue;
                    break;
                case ControlMode.Static:
                    var staticView = new ViewStaticControl
                    {
                        DataContext = new ViewStaticControlViewModel()
                    };
                    CurrentPage = staticView;
                    break;
                case ControlMode.Programmable:
                    var programView = new ViewProgramControl
                    {
                        DataContext = new ViewProgramControlViewModel()
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
