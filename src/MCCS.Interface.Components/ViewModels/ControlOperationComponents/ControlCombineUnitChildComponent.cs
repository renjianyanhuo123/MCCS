using System.Diagnostics;
using System.Text.Json;

using MCCS.Common.Resources.Extensions;
using MCCS.Infrastructure.Communication.NamedPipe;
using MCCS.Infrastructure.Helper;
using MCCS.Interface.Components.Enums;
using MCCS.Interface.Components.ViewModels.ControlCommandPages;
using MCCS.Station.Abstractions.Communication;

namespace MCCS.Interface.Components.ViewModels.ControlOperationComponents
{
    public class ControlModeSelection
    {
        public long ControlModeId { get; set; }
        public string ControlModeName { get; set; } = "";
        public BaseControlViewModel ControlViewModel { get; set; }
    }

    public class ControlCombineUnitChildComponent : BindableBase
    {
        private readonly INotificationService _notificationService;

        public ControlCombineUnitChildComponent(long channelId, string channelName, IEnumerable<ControlModeTypeEnum> controlModes)
        {
            ChannelId = channelId;
            ChannelName = channelName;
            _isValveControlChecked = true;
            ControlModeSelections = controlModes.Select(s => new ControlModeSelection
            {
                ControlModeId = (long)s,
                ControlModeName = EnumHelper.GetDescription(s),
                ControlViewModel = CreateControlViewModel(s),
            }).ToList();
            SelectedControlMode = ControlModeSelections.FirstOrDefault();
            ValveControlCheckedCommand = new AsyncDelegateCommand(ExecuteValveControlCheckedCommand);
        }

        private static BaseControlViewModel CreateControlViewModel(ControlModeTypeEnum controlMode) =>
            controlMode switch
            {
                ControlModeTypeEnum.Manual => new ViewManualControlViewModel(),
                ControlModeTypeEnum.Static => new ViewStaticControlViewModel(),
                ControlModeTypeEnum.Fatigue => new ViewFatigueControlViewModel(),
                ControlModeTypeEnum.Programmable => new ViewProgramControlViewModel(),
                _ => throw new NotImplementedException($"未实现的控制模式视图模型: {controlMode}"),
            };

        public long ChannelId { get; set; }

        public string ChannelName { get; set; }

        private List<ControlModeSelection> _controlModeSelections ;
        public List<ControlModeSelection> ControlModeSelections 
        { 
            get => _controlModeSelections;
            set => SetProperty(ref _controlModeSelections, value);
        }

        private ControlModeSelection? _selectedControlMode; 
        public ControlModeSelection? SelectedControlMode
        {
            get => _selectedControlMode;
            set => SetProperty(ref _selectedControlMode, value);
        }

        private bool _isValveControlChecked;
        public bool IsValveControlChecked
        {
            get => _isValveControlChecked;
            set => SetProperty(ref _isValveControlChecked, value);
        }

        #region Command 
        public DelegateCommand ControlModeSelectionChangedCommand { get; } 
        public AsyncDelegateCommand ValveControlCheckedCommand { get; }
        #endregion

        #region Private Method
        private async Task ExecuteValveControlCheckedCommand()
        {
            if (!IsValveControlChecked) 
            {
                return;
            }

            var payload = JsonSerializer.Serialize(new ValveOperationRequest
            {
                ChannelId = ChannelId,
                Operation = "Close"
            });

            using var client = NamedPipeFactory.CreateClient(options =>
            {
                options.PipeName = NamedPipeCommunication.CommandPipeName;
            });

            while (true)
            {
                var response = await client.SendAsync("operationValveCommand", payload);
                if (response.IsSuccess)
                {
#if DEBUG
                    Debug.WriteLine($"发送成功:{DateTime.Now}");
#endif
                }
            }
        }
        #endregion

        private sealed class ValveOperationRequest
        {
            public long ChannelId { get; init; }
            public string Operation { get; init; } = string.Empty;
        }
    }
}
