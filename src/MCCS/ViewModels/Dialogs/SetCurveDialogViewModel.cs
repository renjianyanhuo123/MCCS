using MaterialDesignThemes.Wpf;

namespace MCCS.ViewModels.Dialogs
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class SetCurveDialogViewModel : BaseDialog
    {
        public const string Tag = "SetCurveDialog";

        #region Command

        public DelegateCommand OkCommand => new(ExecuteOkCommand);
        public DelegateCommand CancelCommand => new(ExecuteCancelCommand);

        #endregion
        
        public SetCurveDialogViewModel()
        {
            //_containerProvider = containerProvider;
        }

        #region private methods

        private void ExecuteOkCommand()
        {
            
        }

        private void ExecuteCancelCommand()
        {
            DialogHost.CloseDialogCommand.Execute(false, null);
        }

        #endregion
    }
}
