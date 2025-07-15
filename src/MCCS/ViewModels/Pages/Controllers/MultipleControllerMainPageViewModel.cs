using MCCS.Core.Devices.Commands;
using MCCS.Events.Controllers;
using System.Collections.ObjectModel;

namespace MCCS.ViewModels.Pages.Controllers
{
    public class MultipleControllerMainPageViewModel : BindableBase
    {
        private readonly IEventAggregator _eventAggregator;

        public MultipleControllerMainPageViewModel(IEventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator;
        }

        #region Property
        /// <summary>
        /// 所有的组合子组件
        /// </summary>
        public ObservableCollection<MultipleControllerChildPageViewModel> Children { get; } = [];

        public string CombineId { get; private set; } = Guid.NewGuid().ToString("N");

        /// <summary>
        /// 是否被选中
        /// </summary>
        private bool _isSelected;

        public bool IsSelected
        {
            get => _isSelected;
            set => SetProperty(ref _isSelected, value);
        }

        private CommandExecuteStatusEnum _currentCommandStatus;
        /// <summary>
        /// 当前指令的执行状态
        /// </summary>
        public CommandExecuteStatusEnum CurrentCommandStatus
        {
            get => _currentCommandStatus;
            set => SetProperty(ref _currentCommandStatus, value);
        }
        #endregion

        #region Command
        /// <summary>
        /// 应用命令
        /// </summary>
        public DelegateCommand ApplyCommand => new(ExecuteApplyCommand);
        /// <summary>
        /// 暂停命令
        /// </summary>
        public DelegateCommand StopCommand => new(ExecuteStopCommand);
        public DelegateCommand<string> UnLockCommand => new(ExecuteUnLockCommand);
        #endregion

        #region private method

        private void ExecuteApplyCommand()
        {
        }

        private void ExecuteStopCommand()
        {

        }

        private void ExecuteUnLockCommand(string combineId)
        {
            _eventAggregator.GetEvent<UnLockCommandEvent>().Publish(new UnLockCommandEventParam(combineId));
        }

        #endregion
    }
}
