using MCCS.Common.Resources.ViewModels;
using MCCS.Models.ProjectManager.Parameters;
using MCCS.ViewModels.MethodManager;

namespace MCCS.ViewModels.ProjectManager
{
    public class ProjectMainPageViewModel : BaseViewModel
    {
        private readonly IRegionManager _regionManager;
        private ProjectOperationParameter? _parameter;

        public ProjectMainPageViewModel(
            IRegionManager regionManager,
            IEventAggregator eventAggregator) : base(eventAggregator)
        {
            _regionManager = regionManager;
            NavigateTestCommand = new DelegateCommand(ExecuteNavigateTestCommand);
            NavigateMethodCommand = new DelegateCommand(ExecuteNavigateMethodCommand); 
        }

        public override void OnNavigatedTo(NavigationContext navigationContext)
        { 
            _parameter = navigationContext.Parameters.GetValue<ProjectOperationParameter>("ProjectInfo");
            // 默认跳转测试界面
            ExecuteNavigateTestCommand();
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
            if (_parameter == null) return;
            var parameter = new NavigationParameters
            {
                { "MethodId", _parameter.MethodId }
            };
            _regionManager.RequestNavigate(GlobalConstant.ProjectNavigateRegionName, new Uri(nameof(ProjectOperationPageViewModel), UriKind.Relative), parameter);
        }

        //public override void OnNavigatedFrom(NavigationContext navigationContext)
        //{
        //    // 离开时移除子Region，避免下次创建新实例时Region名称冲突 
        //    if (_regionManager.Regions.ContainsRegionWithName(GlobalConstant.MethodNavigateRegionName))
        //    {
        //        _regionManager.Regions.Remove(GlobalConstant.MethodNavigateRegionName);
        //    }
        //}

        private void ExecuteNavigateMethodCommand()
        {
            IsTestChecked = false;
            IsMethodChecked = true;
            if (_parameter == null) return;
            var parameter = new NavigationParameters
            {
                { "MethodId", _parameter.MethodId }
            };
            _regionManager.RequestNavigate(GlobalConstant.ProjectNavigateRegionName, new Uri(nameof(MethodContentPageViewModel), UriKind.Relative), parameter);
        }
        #endregion
    }
}
