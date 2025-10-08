using System.Windows.Input;
using MCCS.UserControl;

namespace MCCS.WorkflowSetting.Components.ViewModels
{
    internal class WorkflowStepNodeViewModel : BindingBase
    {

        internal WorkflowStepNodeViewModel()
        {
            OperationNodeClickedCommand = new RelayCommand(ExecuteOperationNodeClickedCommand, _ => true);
            DeleteNodeCommand = new RelayCommand(ExecuteDeleteNodeCommand, _ => true);
            CancelCommand = new RelayCommand(ExecuteCancelCommand, _ => true);
            ConfigueDeleteCommand = new RelayCommand(ExecuteConfigueDeleteCommand, _ => true);
        }

        private bool _isOpen = false;
        public bool IsOpen
        {
            get => _isOpen; 
            set => SetProperty(ref _isOpen, value);
        }

        private bool _isShowShade = false;
        public bool IsShowShade
        {
            get => _isShowShade;
            set => SetProperty(ref _isShowShade, value);
        }

        public ICommand OperationNodeClickedCommand { get; }

        public ICommand DeleteNodeCommand { get; }

        public ICommand CancelCommand { get; }

        public ICommand ConfigueDeleteCommand { get; }

        private void ExecuteConfigueDeleteCommand(object? param)
        {

        }

        private void ExecuteOperationNodeClickedCommand(object? param)
        {
            IsOpen = true; 
        }

        private void ExecuteDeleteNodeCommand(object? param)
        {
            IsShowShade = true;
            IsOpen = false;
        }

        private void ExecuteCancelCommand(object? param)
        {
            IsShowShade = false;
        }
    }
}
