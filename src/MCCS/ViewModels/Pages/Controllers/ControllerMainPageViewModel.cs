using MCCS.Core.Devices.Commands;
using MCCS.Core.Devices.Manager;
using MCCS.Events;
using MCCS.Events.ControlCommand;
using MCCS.Events.Controllers;
using MCCS.Models;
using MCCS.ViewModels.Others.Controllers;
using MCCS.ViewModels.Pages.ControlCommandPages;
using System.Collections.ObjectModel;

namespace MCCS.ViewModels.Pages.Controllers
{
    public class ControllerMainPageViewModel : BaseViewModel
    {
        public const string Tag = "ControllerMainPage";
        private const string None = "None";

        private readonly IEventAggregator _eventAggregator;
        private readonly IRegionManager _regionManager;
        private readonly IDeviceManager _deviceManager; 

        private ObservableCollection<ControllerItemModel> _channels = []; 
        private bool _isShowController = false;
        private bool _isParticipateControl = false;
        // 控制器通道信息字典
        private Dictionary<string, ControlInfo> _controlInfoDic = [];
        private ControlCombineInfo _selectedControlCombineInfo = new()
        {
            CombineChannelId = None,
            CombineChannelName = "无(新增组合)"
        };
        private ObservableCollection<ControlCombineInfo> _controlCombineInfos = [];
        private string _currentChannelId = string.Empty;

        private ControlTypeEnum _controlType = ControlTypeEnum.Single; 
        private int _selectedControlMode = 0;
        private string _currentCombineName = string.Empty;
        
        // private ControlInfo? _lastControlInfoData = null;
        private CommandExecuteStatusEnum _currentCommandStatus;

        public ControllerMainPageViewModel( 
            IRegionManager regionManager,
            IEventAggregator eventAggregator, 
            IDialogService dialogService,
            IDeviceManager deviceManager) : base(eventAggregator, dialogService)
        { 
            _eventAggregator = eventAggregator;
            _regionManager = regionManager;
            _deviceManager = deviceManager;
            eventAggregator.GetEvent<ControlEvent>().Subscribe(RenderChannels);
            _controlCombineInfos.Add(_selectedControlCombineInfo);
        }

        #region Proterty
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
            set 
            {
                if(SetProperty(ref _selectedControlMode, value))
                {
                    ChangeControlInfo();
                }
            }
        }
        /// <summary>
        /// 当前选择的组合
        /// </summary>
        public ControlCombineInfo SelectedControlCombineInfo
        {
            get => _selectedControlCombineInfo;
            set 
            {
                if (SetProperty(ref _selectedControlCombineInfo, value))
                {
                    ChangeControlInfo();
                }
            }
        }
        /// <summary>
        /// 当前组合名称
        /// </summary>
        public string CurrentCombineName
        {
            get => _currentCombineName;
            set 
            {
                if (SetProperty(ref _currentCombineName, value)) 
                {
                    ChangeControlInfo();
                }
            }
        }
        /// <summary>
        /// 所有的组合
        /// </summary>
        public ObservableCollection<ControlCombineInfo> ControlCombineInfos 
        {
            get => _controlCombineInfos;
            set => SetProperty(ref _controlCombineInfos, value);
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
        /// 当前通道Id
        /// </summary>
        public string CurrentChannelId
        {
            get => _currentChannelId;
            set => SetProperty(ref _currentChannelId, value);
        }
        /// <summary>
        /// 是否显示控制区域
        /// </summary>
        public bool IsShowController
        {
            get => _isShowController;
            set => SetProperty(ref _isShowController, value);
        }
        /// <summary>
        /// 是否参与控制
        /// </summary>
        public bool IsParticipateControl 
        {
            get => _isParticipateControl;
            set => SetProperty(ref _isParticipateControl, value);
        }
        /// <summary>
        /// 控制器通道列表
        /// </summary>
        public ObservableCollection<ControllerItemModel> Channels 
        {
            get => _channels;
            set => SetProperty(ref _channels, value);
        }
        #endregion

        #region Command
        public DelegateCommand<string> ParticipateControlCommand => new(ExecuteParticipateControlCommand);

        public DelegateCommand ControlModeSelectionChangedCommand => new(ExecuteControlModeSelectionChangedCommand);
        /// <summary>
        /// 应用命令
        /// </summary>
        public AsyncDelegateCommand ApplyCommand => new(ExecuteApplyCommand);
        /// <summary>
        /// 控制类型切换命令
        /// </summary>
        public DelegateCommand<string> ControlTypeChangedCommand => new(ExecuteControlTypeChangedCommand);
        #endregion

        #region private method 
        private async Task ExecuteApplyCommand(CancellationToken cancellationToken) 
        {
            CurrentCommandStatus = CommandExecuteStatusEnum.Executing;
            var device = _deviceManager.GetDevice(CurrentChannelId) 
                ?? throw new ArgumentNullException(nameof(CurrentChannelId));
            var controlMode = (ControlMode)SelectedControlMode;
            var commandParams = new Dictionary<string, object>();
            switch (controlMode)
            {
                case ControlMode.Manual:
                    break;
                case ControlMode.Static: 
                    commandParams.Add("UnitType", _sharedStaticCommandService.UnitType);
                    commandParams.Add("Speed", _sharedStaticCommandService.Speed);
                    commandParams.Add("TargetValue", _sharedStaticCommandService.TargetValue);
                    break;
                case ControlMode.Programmable:
                    break;
                case ControlMode.Fatigue:
                    break;
                default:
                    break;
            }
            _eventAggregator.GetEvent<ReceivedCommandDataEvent>().Publish(new ReceivedCommandDataEventParam
            {
                Speed = _sharedStaticCommandService.Speed,
                Target = _sharedStaticCommandService.TargetValue,
            });
            await device.SendCommandAsync(new DeviceCommand 
            { 
                DeviceId = CurrentChannelId,
                Type = CommandTypeEnum.SetMove,
                Parameters = commandParams
            }, cancellationToken);
            // 事件完成后订阅
        }
        private void ExecuteControlModeSelectionChangedCommand() 
        {
            var controlMode = (ControlMode)SelectedControlMode;
            string viewName = string.Empty;
            viewName = controlMode switch
            {
                ControlMode.Manual => ViewManualControlViewModel.Tag,
                ControlMode.Static => ViewStaticControlViewModel.Tag,
                ControlMode.Programmable => ViewProgramControlViewModel.Tag,
                ControlMode.Fatigue => ViewFatigueControlViewModel.Tag,
                _ => ViewManualControlViewModel.Tag,
            };
            _regionManager.RequestNavigate(GlobalConstant.ControlCommandRegionName, new Uri(viewName, UriKind.Relative), NavigationCompleted);
        }
        private void NavigationCompleted(NavigationResult result)
        {
        }
        private void ExecuteControlTypeChangedCommand(string controlType) 
        {
            ChangeControlInfo();
        }
        private void OnrevicedControlParameter(ControlParamEventParam param)
        {
            if (param == null) return;

        }
        private void RenderChannels(ControlEventParam param)
        {
            // 寻找需要控制的设备
            //var deviceInfo = _deviceManager.GetDevice(CurrentChannelId)
            //   ?? throw new ArgumentNullException(nameof(CurrentChannelId));
            //var d = deviceInfo.CommandStatusStream.Subscribe(OnChangedCommandStatus);
            Channels.Clear();
            if (param == null) return;
            var success = _controlInfoDic.TryGetValue(param.ChannelId, out var controlInfo);
            controlInfo = success ? controlInfo : new ControlInfo
            {
                ChannelId = param.ChannelId,
                ChannelName = param?.ChannelName ?? "",
                IsCanControl = false,
                ControlType = ControlTypeEnum.Single,
                ControlMode = ControlMode.Manual,
                CombineChannelId = null,
                CombineChannelName = string.Empty
            };
            if (!success) _controlInfoDic.Add(controlInfo.ChannelId, controlInfo);
            SetViewModelValue(controlInfo);
        }

        private void SetViewModelValue(ControlInfo controlInfo) 
        {
            CurrentChannelId = controlInfo.ChannelId;
            ControlType = controlInfo.ControlType;
            SelectedControlMode = (int)controlInfo.ControlMode;
            if (controlInfo.ControlType != ControlTypeEnum.Single)
            {
                SelectedControlCombineInfo = _controlCombineInfos.FirstOrDefault(c => c.CombineChannelId == controlInfo.CombineChannelId)
                    ?? throw new ArgumentNullException();
                CurrentCombineName = SelectedControlCombineInfo.CombineChannelName;
                for (int i = 0; i < SelectedControlCombineInfo.ControlChannels.Count; i++)
                {
                    Channels.Add(new ControllerItemModel
                    {
                        Index = i + 1,
                        ChannelId = SelectedControlCombineInfo.ControlChannels[i].ChannelId,
                        ChannelName = SelectedControlCombineInfo.ControlChannels[i].ChannelName,
                    });
                }
            }
            else 
            {
                Channels.Add(new ControllerItemModel
                {
                    Index = 1,
                    ChannelId = controlInfo.ChannelId,
                    ChannelName = controlInfo.ChannelName,
                });
            }
            IsShowController = true;
            IsParticipateControl = true;
        }

        private void ChangeControlInfo() 
        {
            if (string.IsNullOrEmpty(CurrentChannelId)) return;
            var success = _controlInfoDic.TryGetValue(CurrentChannelId, out var controlInfo);
            if (!success) throw new ArgumentNullException(nameof(CurrentChannelId));
            controlInfo.ControlType = ControlType;
            controlInfo.ControlMode = (ControlMode)SelectedControlMode;
            if (ControlType == ControlTypeEnum.Combine)
            {
                if (SelectedControlCombineInfo.CombineChannelId == None)
                {
                    if (!string.IsNullOrEmpty(CurrentCombineName))
                    {
                        // 选择新增组合
                        controlInfo.CombineChannelId = Guid.NewGuid().ToString("N");
                        controlInfo.CombineChannelName = CurrentCombineName;
                        var newCombine = new ControlCombineInfo
                        {
                            CombineChannelId = controlInfo.CombineChannelId,
                            CombineChannelName = controlInfo.CombineChannelName
                        };
                        newCombine.ControlChannels.Add(controlInfo);
                        RemoveFromOldCombine(controlInfo.ChannelId);
                        if (!_controlCombineInfos.Any(c => c.CombineChannelName == newCombine.CombineChannelName))
                        {
                            _controlCombineInfos.Add(newCombine);
                        }
                    }
                }
                else
                {
                    var combineInfo = _controlCombineInfos.FirstOrDefault(c => c.CombineChannelId == SelectedControlCombineInfo.CombineChannelId);
                    controlInfo.CombineChannelId = combineInfo?.CombineChannelId;
                    controlInfo.CombineChannelName = combineInfo?.CombineChannelName;
                    if (!combineInfo.ControlChannels.Any(c => c.ChannelId == controlInfo.ChannelId))
                    {
                        combineInfo?.ControlChannels.Add(controlInfo);
                    }
                }
            }
            else 
            {
                RemoveFromOldCombine(controlInfo.ChannelId);
            }
        }

        /// <summary>
        /// 监听指令状态变化
        /// </summary>
        /// <param name="response"></param>
        private void OnChangedCommandStatus(CommandResponse response)
        {

        }

        private void RemoveFromOldCombine(string channelId) 
        {
            var removeObj = _controlCombineInfos.FirstOrDefault(c => c.ControlChannels.Any(s => s.ChannelId == channelId));
            removeObj?.ControlChannels.RemoveAll(c => c.ChannelId == channelId);
        }

        private void ExecuteParticipateControlCommand(string channelId) 
        {
            _eventAggregator.GetEvent<InverseControlEvent>().Publish(new InverseControlEventParam
            {
                DeviceId = channelId,
            });
            _controlInfoDic.Remove(channelId, out var controlInfo);
            if (controlInfo?.DeviceSubscription != null)
            {
                controlInfo.DeviceSubscription.Dispose();
            }
            IsShowController = false;
        }
        #endregion
    }
}
