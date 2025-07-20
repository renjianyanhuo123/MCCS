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
    }
}
