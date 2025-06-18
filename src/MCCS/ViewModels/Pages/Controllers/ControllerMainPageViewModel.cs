using MCCS.Core.Devices.Commands;
using MCCS.Core.Devices.Manager;
using MCCS.Events;
using MCCS.Events.Controllers;
using MCCS.Models;
using MCCS.Services.ControlCommand;
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
        private readonly ISharedStaticCommandService _sharedStaticCommandService;

        private ObservableCollection<ControllerItemModel> _channels = []; 
        private bool _isShowController = false;
        private bool _isParticipateControl = false;
        // 控制器通道信息字典
        private Dictionary<string, ControlInfo> _controlInfoDic = [];
        private ObservableCollection<ControlCombineInfo> _controlCombineInfos =
        [
            new() {
                CombineChannelId = None,
                CombineChannelName = "无(新增组合)"
            }
        ];
        private string _currentChannelId = string.Empty;

        private ControlTypeEnum _controlType = ControlTypeEnum.Single; 
        private int _selectedControlMode = 0;
        private string _currentCombineName = string.Empty;
        private ControlCombineInfo _selectedControlCombineInfo;
        private ControlInfo? _lastControlInfoData = null;

        public ControllerMainPageViewModel(
            ISharedStaticCommandService sharedStaticCommandService,
            IRegionManager regionManager,
            IEventAggregator eventAggregator, 
            IDialogService dialogService,
            IDeviceManager deviceManager) : base(eventAggregator, dialogService)
        {
            _sharedStaticCommandService = sharedStaticCommandService;
            _eventAggregator = eventAggregator;
            _regionManager = regionManager;
            _deviceManager = deviceManager;
            eventAggregator.GetEvent<ControlEvent>().Subscribe(RenderChannels); 
        }

        #region Proterty
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

        public AsyncDelegateCommand ApplyCommand => new(ExecuteApplyCommand);
        #endregion

        #region private method
        private bool _isExecuting = false;
        private async Task ExecuteApplyCommand() 
        {
            if (_isExecuting) return;
            _isExecuting = true;
            var device = _deviceManager.GetDevice(CurrentChannelId) ?? throw new ArgumentNullException(nameof(CurrentChannelId));
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
            });
            _isExecuting = false;
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
        private void RenderChannels(ControlEventParam param)
        {
            // (1) 首先收集下之前界面的所有的控制信息
            if (IsShowController) 
            {
                _lastControlInfoData = GetViewModelValue(param);
                if (_controlInfoDic.ContainsKey(_lastControlInfoData.ChannelId))
                {
                    _controlInfoDic[_lastControlInfoData.ChannelId] = _lastControlInfoData;
                }
                else
                {
                    _controlInfoDic.Add(_lastControlInfoData.ChannelId, _lastControlInfoData);
                }
            }
            Channels.Clear();
            if (param == null) return;
            var success = _controlInfoDic.TryGetValue(param.ChannelId, out var controlInfo);
            controlInfo = success ? controlInfo : new ControlInfo
            {
                ChannelId = param.ChannelId,
                ChannelName = param.ChannelName,
                IsCanControl = false,
                ControlType = ControlTypeEnum.Single,
                ControlMode = ControlMode.Manual,
                CombineChannelId = null,
                CombineChannelName = string.Empty
            };
            SetViewModelValue(controlInfo);
        }

        private void SetViewModelValue(ControlInfo controlInfo) 
        {
            CurrentChannelId = controlInfo.ChannelId;
            ControlType = controlInfo.ControlType;
            if (controlInfo.CombineChannelId != null)
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
            SelectedControlMode = (int)controlInfo.ControlMode;
            IsShowController = true;
            IsParticipateControl = true;
        }

        private ControlInfo GetViewModelValue(ControlEventParam param) 
        {
            var controlInfo = new ControlInfo
            {
                ChannelId = string.IsNullOrEmpty(CurrentChannelId) ? param.ChannelId : CurrentChannelId,
                ChannelName = Channels.FirstOrDefault(c => c.ChannelId == CurrentChannelId)?.ChannelName ?? param.ChannelName ?? "",
                IsCanControl = IsParticipateControl,
                ControlType = ControlType,
                ControlMode = (ControlMode)SelectedControlMode,
            };
            if (ControlType == ControlTypeEnum.Combine)
            {
                if (SelectedControlCombineInfo.CombineChannelId == None)
                {
                    // 选择新增组合
                    controlInfo.CombineChannelId = Guid.NewGuid().ToString("N");
                    controlInfo.CombineChannelName = string.IsNullOrEmpty(CurrentCombineName) ? DateTime.Now.ToString("yyMMdd_hhmmss") : CurrentCombineName;
                    var newCombine = new ControlCombineInfo
                    {
                        CombineChannelId = controlInfo.CombineChannelId,
                        CombineChannelName = controlInfo.CombineChannelName
                    };
                    newCombine.ControlChannels.Add(controlInfo);
                    RemoveFromOldCombine(controlInfo.ChannelId);
                    _controlCombineInfos.Add(newCombine);
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
            return controlInfo;
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
            IsShowController = false;
        }
        #endregion
    }
}
