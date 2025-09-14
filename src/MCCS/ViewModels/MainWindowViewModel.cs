using MCCS.Core.Repositories;
using MCCS.Events.StartUp;
using MCCS.ViewModels.Pages;

namespace MCCS.ViewModels
{
    public class MainWindowViewModel : BaseViewModel
    {
        private readonly IRegionManager _regionManager;
        private readonly ISystemMenuRepository _systemMenuRepository;
        private readonly IContainerProvider _containerProvider;

        #region 页面属性    
        #endregion 

        #region 命令执行  
        #endregion

        public MainWindowViewModel(
            IContainerProvider containerProvider, 
            IRegionManager regionManager, 
            IEventAggregator eventAggregator) : base(eventAggregator)
        {
            _regionManager = regionManager;
            regionManager.RequestNavigate(GlobalConstant.StartUpRegionName, new Uri(SplashPageViewModel.Tag, UriKind.Relative));
            _eventAggregator.GetEvent<FinishStartUpNotificationEvent>().Subscribe(JumpToMainPage);
        }

        private void JumpToMainPage(FinishStartUpNotificationEventParam param)
        {
            if (param.IsSuccess)
            {
                _regionManager.RequestNavigate(GlobalConstant.StartUpRegionName, new Uri(MainContentPageViewModel.Tag, UriKind.Relative));
            }
        }
    }
}
