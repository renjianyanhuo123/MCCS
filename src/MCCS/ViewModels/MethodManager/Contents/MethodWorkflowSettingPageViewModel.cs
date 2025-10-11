namespace MCCS.ViewModels.MethodManager.Contents
{
    public sealed class MethodWorkflowSettingPageViewModel : BaseViewModel
    {
        public const string Tag = "MethodWorkflowSettingPage";

        private long _methodId = -1; 
        private readonly IRegionManager _regionManager;

        

        public MethodWorkflowSettingPageViewModel(IEventAggregator eventAggregator, 
            IRegionManager regionManager) : base(eventAggregator)
        {
            _regionManager = regionManager; 
            LoadCommand = new DelegateCommand<object>(ExecuteLoadCommand); 
        }

        public override void OnNavigatedTo(NavigationContext navigationContext)
        {
            _methodId = navigationContext.Parameters.GetValue<long>("MethodId");
        }

        #region Command 
        public DelegateCommand<object> LoadCommand { get; }  
        #endregion

        #region Private Method  
        private void ExecuteLoadCommand(object param)
        {
            if (_methodId == -1) throw new ArgumentNullException("No MethodId!");  
        }
        #endregion
    }
}
