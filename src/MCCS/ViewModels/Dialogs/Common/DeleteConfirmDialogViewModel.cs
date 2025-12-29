using MaterialDesignThemes.Wpf;

namespace MCCS.ViewModels.Dialogs.Common
{
    public class DeleteConfirmDialogViewModel : BaseDialog
    {
        private string? _dialogRootName = "RootDialog";

        #region Property 
        private string? _showContent = null; 
        public string? ShowContent
        {
            get => _showContent;
            set => SetProperty(ref _showContent, value);
        }
        #endregion

        #region Command
        private DelegateCommand<string>? _closeDialogCommand;
        public DelegateCommand<string> CloseDialogCommand =>
            _closeDialogCommand ??= new DelegateCommand<string>(CloseDialog);
        #endregion
        protected virtual void CloseDialog(string parameter)
        {
            var result = ButtonResult.None;
            DialogHost.Close(_dialogRootName);
            if (parameter?.ToLower() == "true")
                result = ButtonResult.OK;
            else if (parameter?.ToLower() == "false")
                result = ButtonResult.Cancel;
            RaiseRequestClose(new DialogResult(result));
        }
        public override void OnDialogOpened(IDialogParameters parameters)
        {
            ShowContent = parameters.GetValue<string>("ShowContent");
            Title = parameters.GetValue<string>("Title");
            _dialogRootName = parameters.GetValue<string>("RootDialogName");
        } 
    }
}
