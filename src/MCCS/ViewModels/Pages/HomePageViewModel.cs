using MCCS.Common.Resources.ViewModels;
using MCCS.Events;
using MCCS.ViewModels.MethodManager;
using MCCS.ViewModels.ProjectManager;

namespace MCCS.ViewModels.Pages
{
    public class HomePageViewModel : BaseViewModel
    {
        public const string Tag = "HomePage";

        private readonly IRegionManager _regionManager;

        public HomePageViewModel( 
            IEventAggregator eventAggregator,
            IRegionManager regionManager,
            IDialogService dialogService) : base(eventAggregator, dialogService)
        {
            _regionManager = regionManager;
            JumpToProjectCommand = new DelegateCommand(ExecuteJumpToProjectCommand);
            JumpToMethodCommand = new DelegateCommand(ExecuteJumpToMethodCommand);
        }

        #region 页面属性 
        #endregion

        #region 命令 
        public DelegateCommand JumpToProjectCommand { get; }
        public DelegateCommand JumpToMethodCommand { get; }
        #endregion

        #region 私有方法 
        private void ExecuteJumpToProjectCommand()
        {
            _regionManager.RequestNavigate(GlobalConstant.MainContentRegionName, new Uri(nameof(ProjectListPageViewModel), UriKind.Relative));
            _eventAggregator.GetEvent<NotificationCancelSelectedEvent>().Publish(new NotificationCancelSelectedEventParam());
        }
        private void ExecuteJumpToMethodCommand()
        {
            _regionManager.RequestNavigate(GlobalConstant.MainContentRegionName, new Uri(MethodMainPageViewModel.Tag, UriKind.Relative));
            _eventAggregator.GetEvent<NotificationCancelSelectedEvent>().Publish(new NotificationCancelSelectedEventParam());
        }
        #endregion
    }
}
