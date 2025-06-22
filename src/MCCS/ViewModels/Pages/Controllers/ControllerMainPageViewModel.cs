using MCCS.Core.Devices.Commands;
using MCCS.Core.Devices.Manager;
using MCCS.Events;
using MCCS.Events.ControlCommand;
using MCCS.Events.Controllers;
using MCCS.Models;
using MCCS.Models.ControlCommand;
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
        private string _currentChannelName = string.Empty;

        private object? _tempSaveControlParamInfo;
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
            // eventAggregator.GetEvent<ControlEvent>().Subscribe(RenderChannels);
            eventAggregator.GetEvent<ControlParamEvent>().Subscribe(OnrevicedControlParam);
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
            set => SetProperty(ref _selectedControlMode, value);
        }
        /// <summary>
        /// 当前选择的组合
        /// </summary>
        public ControlCombineInfo SelectedControlCombineInfo
        {
            get => _selectedControlCombineInfo;
            set => SetProperty(ref _selectedControlCombineInfo, value);
        }
        /// <summary>
        /// 当前组合名称
        /// </summary>
        public string CurrentCombineName
        {
            get => _currentCombineName;
            set => SetProperty(ref _currentCombineName, value);
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
        /// 当前通道名称
        /// </summary>
        public string CurrentChannelName 
        {
            get => _currentChannelName;
            set => SetProperty(ref _currentChannelName, value);
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
        public DelegateCommand ControlCombineSelectionChangedCommand => new(ExecuteControlCombineSelectionChangedCommand);
        /// <summary>
        /// 是否参与控制
        /// </summary>
        public DelegateCommand<string> ParticipateControlCommand => new(ExecuteParticipateControlCommand);
        /// <summary>
        /// 控制模式切换命令
        /// </summary>
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

        /// <summary>
        /// (2)导航到页面的
        /// </summary>
        /// <param name="navigationContext"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public override void OnNavigatedTo(NavigationContext navigationContext)
        {
            var channelId = navigationContext.Parameters.GetValue<string>("ChannelId");
            var channelName = navigationContext.Parameters.GetValue<string>("ChannelName");
            CurrentChannelId = channelId ?? throw new ArgumentNullException(nameof(channelId));
            CurrentChannelName = channelName;
            Channels.Clear(); 
            var success = _controlInfoDic.TryGetValue(channelId, out var controlInfo);
            controlInfo = success ? controlInfo : new ControlInfo
            {
                ChannelId = channelId,
                ChannelName = channelName ?? "",
                IsCanControl = false,
                ControlType = ControlTypeEnum.Single,
                ControlMode = ControlMode.Manual,
                CombineChannelId = null,
                CombineChannelName = string.Empty
            };
            if (!success) _controlInfoDic.Add(controlInfo.ChannelId, controlInfo);
            SetViewModelValue(controlInfo);
            var controlInfoTemp = _controlInfoDic[CurrentChannelId];
            var sendParams = new NavigationParameters();
            if (ControlType == ControlTypeEnum.Single)
            {
                sendParams.Add("ControlModel", controlInfoTemp?.ControlParams);
            }
            else
            {
                var temp = _controlCombineInfos.FirstOrDefault(c => c.CombineChannelId == controlInfoTemp.CombineChannelId)?.ControlParams;
                sendParams.Add("ControlModel", temp);
            }
            _regionManager.RequestNavigate(GlobalConstant.ControlCommandRegionName, new Uri(GetViewName(), UriKind.Relative), NavigationCompleted, sendParams);
        }

        /// <summary>
        /// (1)离开界面
        /// </summary>
        /// <param name="navigationContext"></param>
        public override void OnNavigatedFrom(NavigationContext navigationContext)
        {
            ChangeControlInfo();
        }

        #region private method 
        private async Task ExecuteApplyCommand(CancellationToken cancellationToken) 
        {
            CurrentCommandStatus = CommandExecuteStatusEnum.Executing;
            var device = _deviceManager.GetDevice(CurrentChannelId) 
                ?? throw new ArgumentNullException(nameof(CurrentChannelId));
            var controlMode = (ControlMode)SelectedControlMode;
            var commandParams = new Dictionary<string, object>();
            var model = _controlInfoDic[CurrentChannelId].ControlParams;
            switch (controlMode)
            {
                case ControlMode.Manual:
                    break;
                case ControlMode.Static:
                    var temp = model as StaticControlModel
                        ?? throw new ArgumentNullException(nameof(model));
                    _eventAggregator.GetEvent<ReceivedCommandDataEvent>().Publish(new ReceivedCommandDataEventParam
                    {
                        Speed = temp.Speed,
                        Target = temp.TargetValue,
                    });
                    commandParams.Add("UnitType", temp.UnitType);
                    commandParams.Add("Speed", temp.Speed);
                    commandParams.Add("TargetValue", temp.TargetValue);
                    break;
                case ControlMode.Programmable:
                    break;
                case ControlMode.Fatigue:
                    break;
                default:
                    break;
            } 
            
            await device.SendCommandAsync(new DeviceCommand 
            { 
                DeviceId = CurrentChannelId,
                Type = CommandTypeEnum.SetMove,
                Parameters = commandParams
            }, cancellationToken);
            // 事件完成后订阅
        }

        private string GetViewName() 
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
            return viewName;
        }
        private void ExecuteControlModeSelectionChangedCommand() 
        {
            _regionManager.RequestNavigate(GlobalConstant.ControlCommandRegionName, new Uri(GetViewName(), UriKind.Relative), NavigationCompleted);
        } 
        private void NavigationCompleted(NavigationResult result)
        {
            // ChangeControlInfo();
        }
        /// <summary>
        /// 控制类型切换触发
        /// </summary>
        /// <param name="controlType"></param>
        private void ExecuteControlTypeChangedCommand(string controlType) 
        {
            if (controlType == "Combine") 
            {
                // 组合模式下，强制切换为和组合状态一致
                SelectedControlCombineInfo = _controlCombineInfos.FirstOrDefault(c => c.CombineChannelId == "None");
            }
        }

        private void SetViewModelValue(ControlInfo controlInfo) 
        {
            CurrentChannelId = controlInfo.ChannelId;
            ControlType = controlInfo.ControlType;
            SelectedControlMode = (int)controlInfo.ControlMode;
            if (controlInfo.ControlType != ControlTypeEnum.Single)
            {
                SelectedControlCombineInfo = _controlCombineInfos.First(c => c.CombineChannelId == controlInfo.CombineChannelId);
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
            controlInfo.ChannelName = CurrentChannelName;
            controlInfo.ControlMode = (ControlMode)SelectedControlMode;
            RemoveFromOldCombine(controlInfo.ChannelId);
            if (ControlType != ControlTypeEnum.Combine) 
            {
                controlInfo.ControlParams = _tempSaveControlParamInfo;
                return;
            }
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
                        CombineChannelName = controlInfo.CombineChannelName,
                        ControlMode = (ControlMode)SelectedControlMode
                    };
                    newCombine.ControlParams = _tempSaveControlParamInfo;
                    newCombine.ControlChannels.Add(controlInfo);
                    if (!_controlCombineInfos.Any(c => c.CombineChannelName == newCombine.CombineChannelName))
                    {
                        _controlCombineInfos.Add(newCombine);
                    }
                }
            }
            else
            {
                var combineInfo = _controlCombineInfos.First(c => c.CombineChannelId == SelectedControlCombineInfo.CombineChannelId);
                controlInfo.CombineChannelId = combineInfo?.CombineChannelId;
                controlInfo.CombineChannelName = combineInfo?.CombineChannelName;
                combineInfo.ControlParams = _tempSaveControlParamInfo;
                if (!combineInfo.ControlChannels.Any(c => c.ChannelId == controlInfo.ChannelId))
                {
                    combineInfo?.ControlChannels.Add(controlInfo);
                }
            }
        }

        /// <summary>
        /// 接受数据(在ChangeControlInfo 方法执行前执行)
        /// </summary>
        /// <param name="param"></param>
        private void OnrevicedControlParam(ControlParamEventParam param) 
        {
            if (param == null || string.IsNullOrEmpty(CurrentChannelId)) return;
            _tempSaveControlParamInfo = param.Param;
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

        private void ExecuteControlCombineSelectionChangedCommand() 
        {
            // 组合模式下，强制切换为和组合状态一致
            var temp = _controlCombineInfos.FirstOrDefault(c => c.CombineChannelId == SelectedControlCombineInfo.CombineChannelId);
            SelectedControlMode = (int)(temp?.ControlMode ?? 0);
            var sendParams = new NavigationParameters(); 
            sendParams.Add("ControlModel", temp?.ControlParams);
            _regionManager.RequestNavigate(GlobalConstant.ControlCommandRegionName, new Uri(GetViewName(), UriKind.Relative), NavigationCompleted, sendParams);
        }
        #endregion
    }
}
