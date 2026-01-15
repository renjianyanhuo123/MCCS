using MCCS.Infrastructure.Helper;
using MCCS.Interface.Components.Enums;
using MCCS.Interface.Components.ViewModels.ControlCommandPages;

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


        public ControlCombineUnitChildComponent(long channelId, string channelName, IEnumerable<ControlModeTypeEnum> controlModes)
        {
            ChannelId = channelId;
            ChannelName = channelName;
            ControlModeSelections = controlModes.Select(s => new ControlModeSelection
            {
                ControlModeId = (long)s,
                ControlModeName = EnumHelper.GetDescription(s),
                ControlViewModel = CreateControlViewModel(s),
            }).ToList();
            SelectedControlMode = ControlModeSelections.FirstOrDefault();
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

        #region Command 
        public DelegateCommand ControlModeSelectionChangedCommand { get; } 
        #endregion
    }
}
