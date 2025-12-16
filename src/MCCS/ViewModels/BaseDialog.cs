namespace MCCS.ViewModels;

public class BaseDialog : BindableBase, IDialogAware
{
    private string _title = string.Empty;
    public string Title
    {
        get => _title;
        set => SetProperty(ref _title, value);
    }

    public bool CanCloseDialog() => true;
    public DelegateCommand CloseCommand => new(ExecuteCloseCommand);

    private void ExecuteCloseCommand()
    {
        RequestClose.Invoke(new DialogResult(ButtonResult.OK));
    }
    
    public virtual void OnDialogClosed()
    {
    }

    public  virtual void OnDialogOpened(IDialogParameters parameters)
    {
    }

    public DialogCloseListener RequestClose { get; }
}