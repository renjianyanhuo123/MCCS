using MCCS.Core.Devices.Commands;
using MCCS.Models;
using MCCS.ViewModels.Pages.ControlCommandPages;
using MCCS.Views.Pages.ControlCommandPages;

namespace MCCS.ViewModels.Pages.Controllers
{
    public class MultipleControllerChildPageViewModel : BindableBase
    { 
        private bool _isShowController = false;
        private bool _isParticipateControl = false; 

        private ControlTypeEnum _controlType = ControlTypeEnum.Single;
        private int _selectedControlMode = 0;
        private string _currentChannelName = string.Empty;

        private CommandExecuteStatusEnum _currentCommandStatus;
         
        public MultipleControllerChildPageViewModel(
            long currentModelId)
        {
            CurrentModelId = currentModelId;
            CurrentChannelName = "多力/位移控制通道";
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
        public string CurrentChannelName
        {
            get => _currentChannelName;
            set => SetProperty(ref _currentChannelName, value);
        }  
        #endregion

        #region Command  
        /// <summary>
        /// 控制模式切换命令
        /// </summary>
        public DelegateCommand ControlModeSelectionChangedCommand => new(ExecuteControlModeSelectionChangedCommand); 
        #endregion 

        #region private method   
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
        #endregion
    }
}
