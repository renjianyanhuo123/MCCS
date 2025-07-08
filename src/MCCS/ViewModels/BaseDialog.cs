namespace MCCS.ViewModels;

public class BaseDialog : BindableBase, IDialogAware
{
    public bool CanCloseDialog()
    {
        return true;
    }
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