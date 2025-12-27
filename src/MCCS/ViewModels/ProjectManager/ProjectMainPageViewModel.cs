using MCCS.ViewModels.MethodManager;

namespace MCCS.ViewModels.ProjectManager
{
    public class ProjectMainPageViewModel : BaseViewModel
    {
        private readonly IRegionManager _regionManager;

        public ProjectMainPageViewModel(
            IRegionManager regionManager,
            IEventAggregator eventAggregator) : base(eventAggregator)
        {
            _regionManager = regionManager;
            NavigateTestCommand = new DelegateCommand(ExecuteNavigateTestCommand);
            NavigateMethodCommand = new DelegateCommand(ExecuteNavigateMethodCommand);
        }

        #region Property 
        private bool _isTestChecked;
        public bool IsTestChecked
        {
            get => _isTestChecked;
            set => SetProperty(ref _isTestChecked, value);
        }

        private bool _isMethodChecked;
        public bool IsMethodChecked
        {
            get => _isMethodChecked;
            set => SetProperty(ref _isMethodChecked, value);
        }
        #endregion

        #region Command
        public DelegateCommand NavigateTestCommand { get; }
        public DelegateCommand NavigateMethodCommand { get; }

        #endregion

        #region Private Method
        private void ExecuteNavigateTestCommand()
        {
            IsTestChecked = true;
            IsMethodChecked = false;
            _regionManager.RequestNavigate(GlobalConstant.ProjectNavigateRegionName, new Uri(nameof(ProjectOperationPageViewModel), UriKind.Relative));
        }

        private void ExecuteNavigateMethodCommand()
        {
            IsTestChecked = false;
            IsMethodChecked = true;
            var parameter = new NavigationParameters
            {
                //{ "MethodId", _methodId }
            };
            _regionManager.RequestNavigate(GlobalConstant.ProjectNavigateRegionName, new Uri(nameof(MethodContentPageViewModel), UriKind.Relative), parameter);
        }
        #endregion
    }
}
