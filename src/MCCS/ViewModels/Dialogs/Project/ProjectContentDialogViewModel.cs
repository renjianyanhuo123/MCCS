using MCCS.ViewModels.Dialogs.Common;

namespace MCCS.ViewModels.Dialogs.Project
{
    public class ProjectContentDialogViewModel : BaseDialog
    { 
        private readonly IDialogService _dialogService;

        public ProjectContentDialogViewModel(IDialogService dialogService)
        {
            _dialogService = dialogService;
        }

        #region Command
        private AsyncDelegateCommand? _closeDialogCommand;
        public AsyncDelegateCommand CloseDialogCommand =>
            _closeDialogCommand ??= new AsyncDelegateCommand(CloseDialog);
        #endregion

        private object? _innerViewModel; 
        public object? InnerViewModel
        {
            get => _innerViewModel; 
            set => SetProperty(ref _innerViewModel, value);
        }

        public override void OnDialogOpened(IDialogParameters parameters)
        {
            var contentViewModel = parameters.GetValue<object>("ContentViewModel");
            InnerViewModel = contentViewModel;
            Title = parameters.GetValue<string>("Title");
        }

        protected virtual async Task CloseDialog()
        {
            var dialogParameters = new DialogParameters
            {
                { "Title", "是否确认关闭该非占位组件?" },
                { "ShowContent", "注意: 关闭该组件后，则在当前界面中将会消失,必须重新刷新试验界面才会出现！"},
                { "RootDialogName", "ProjectComponentDialog" }
            };
            var result = await _dialogService.ShowDialogAsync(nameof(DeleteConfirmDialogViewModel), dialogParameters);
            if (InnerViewModel == null) return;
            if (result.Result == ButtonResult.OK)
            {
                var parameters = new DialogParameters
                {
                    { "ContentViewModel", InnerViewModel }
                };
                RaiseRequestClose(new DialogResult
                {
                    Parameters = parameters
                });
            }
        } 

    }
}
