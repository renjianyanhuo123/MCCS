namespace MCCS.ViewModels.Pages.SystemManager
{
    public class SystemManagerViewModel : BaseViewModel
    {
        public const string Tag = "SystemManager";

        private string _currentSelectedPage;

        private readonly IRegionManager _regionManager;

        public SystemManagerViewModel(IRegionManager regionManager, IEventAggregator eventAggregator, IDialogService? dialogService) 
            : base(eventAggregator, dialogService)
        {
            _currentSelectedPage = PermissionManagementViewModel.Tag;
            _regionManager = regionManager;
        }

        #region Property
        public string CurrentSelectedPage
        {
            get => _currentSelectedPage;
            set => SetProperty(ref _currentSelectedPage, value);
        }
        #endregion

        #region Command
        public DelegateCommand<string> JumpToCommand => new(ExecuteJumpToCommand);
        #endregion

        #region private method
        private void ExecuteJumpToCommand(string pageId)
        {
            _regionManager.RequestNavigate(GlobalConstant.SystemManagerRegionName, new Uri(pageId, UriKind.Relative));
        }
        #endregion
    }
}
