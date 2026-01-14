using MCCS.Common.Resources.ViewModels;

namespace MCCS.Interface.Components.ViewModels.ControlOperationComponents
{
    /// <summary>
    /// 控制操作组件 ViewModel
    /// </summary>
    public class ControlOperationComponentPageViewModel : BaseViewModel
    {
        public ControlOperationComponentPageViewModel(IEventAggregator eventAggregator) : base(eventAggregator)
        {
            InitializeCommands();
        }

        public ControlOperationComponentPageViewModel(IEventAggregator eventAggregator, IDialogService? dialogService) : base(eventAggregator, dialogService)
        {
            InitializeCommands();
        }

        /// <summary>
        /// 设置命令
        /// </summary>
        public DelegateCommand SettingsCommand { get; private set; } = null!;

        /// <summary>
        /// 刷新命令
        /// </summary>
        public DelegateCommand RefreshCommand { get; private set; } = null!;

        /// <summary>
        /// 帮助命令
        /// </summary>
        public DelegateCommand HelpCommand { get; private set; } = null!;

        private void InitializeCommands()
        {
            SettingsCommand = new DelegateCommand(ExecuteSettingsCommand);
            RefreshCommand = new DelegateCommand(ExecuteRefreshCommand);
            HelpCommand = new DelegateCommand(ExecuteHelpCommand);
        }

        private void ExecuteSettingsCommand()
        {
            // TODO: 实现设置功能
        }

        private void ExecuteRefreshCommand()
        {
            // TODO: 实现刷新功能
        }

        private void ExecuteHelpCommand()
        {
            // TODO: 实现帮助功能
        }
    }
}
