namespace MCCS.ViewModels.Pages.SystemManager
{
    public class HardwareSettingPageViewModel : BaseViewModel
    {
        public const string Tag = "HardwareSetting";

        public HardwareSettingPageViewModel(IEventAggregator eventAggregator) : base(eventAggregator)
        {
        }

        public HardwareSettingPageViewModel(IEventAggregator eventAggregator, IDialogService dialogService) : base(eventAggregator, dialogService)
        {
        }
    }
}
