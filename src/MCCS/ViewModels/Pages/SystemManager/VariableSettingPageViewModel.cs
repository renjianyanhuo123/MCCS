namespace MCCS.ViewModels.Pages.SystemManager
{
    public sealed class VariableSettingPageViewModel : BaseViewModel
    {
        public const string Tag = "VariableSetting";

        public VariableSettingPageViewModel(IEventAggregator eventAggregator) : base(eventAggregator)
        {
        }

        public VariableSettingPageViewModel(IEventAggregator eventAggregator, IDialogService? dialogService) : base(eventAggregator, dialogService)
        {
        }

        public override void OnNavigatedTo(NavigationContext navigationContext)
        { 

        }

        #region Property
        public long VariableId { get; set; }

        private string _variableName = string.Empty;
        public string VariableName
        {
            get => _variableName;
            set => SetProperty(ref _variableName, value);
        }

        private string _internalId = string.Empty;
        public string InternalId
        {
            get => _internalId;
            set => SetProperty(ref _variableName, value);
        }

        private bool _isShowable = false;
        public bool IsShowable
        {
            get => _isShowable;
            set => SetProperty(ref _isShowable, value);
        }

        private bool _isCanControl = false;
        public bool IsCanControl
        {
            get => _isCanControl;
            set => SetProperty(ref _isCanControl, value);
        }

        private bool _isCanCalibrate = false;
        public bool IsCanCalibrate
        {
            get => _isCanCalibrate;
            set => SetProperty(ref _isCanCalibrate, value);
        }

        private bool _isCanSetLimit = false;
        public bool IsCanSetLimit
        {
            get => _isCanSetLimit;
            set => SetProperty(ref _isCanSetLimit, value);
        }

        #endregion

        #region Command

        #endregion
    }
}
