namespace MCCS.ViewModels.Pages.SystemManager
{
    public class PermissionManagementViewModel : BaseViewModel
    {
        public const string Tag = "PermissionManagement";

        public PermissionManagementViewModel(IEventAggregator eventAggregator) : base(eventAggregator)
        {
        }

        public PermissionManagementViewModel(IEventAggregator eventAggregator, IDialogService? dialogService) : base(eventAggregator, dialogService)
        {
        }
    }
}
