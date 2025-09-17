using MCCS.Views.Dialogs;
using System.Windows;

namespace MCCS.Services.DialogServices
{
    public sealed class ExtendedDialogService(IDialogService dialogService, IContainerProvider containerProvider)
    {
        private readonly IDialogService _dialogService;
        private readonly IContainerProvider _containerProvider; 
    }
}
