namespace MCCS.Services.DialogServices
{
    public interface IExtendedDialogService : IDialogService
    {
        void ShowNonModalDialog(string name, IDialogParameters parameters = null, Action<IDialogResult> callback = null);
    }
}
