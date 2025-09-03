using MaterialDesignThemes.Wpf;
using MCCS.Events.Common;

namespace MCCS.ViewModels.Dialogs.Common
{
    public class DeleteConfirmDialogViewModel : BaseViewModel
    {
        public const string Tag = "DeleteConfirmDialog";

        public DeleteConfirmDialogViewModel(IEventAggregator eventAggregator) : base(eventAggregator)
        {
        }

        #region Command
        public DelegateCommand CancelCommand => new(ExecuteCancelCommand);
        public DelegateCommand ConfirmCommand => new(ExecuteConfirmCommand);
        #endregion

        #region Private Method
        private void ExecuteCancelCommand()
        {
            DialogHost.Close("RootDialog", new DialogConfirmEvent()
            {
                IsConfirmed = false
            });
        }
        private void ExecuteConfirmCommand()
        {
            DialogHost.Close("RootDialog", new DialogConfirmEvent()
            {
                IsConfirmed = true
            });
        }
        #endregion

    }
}
