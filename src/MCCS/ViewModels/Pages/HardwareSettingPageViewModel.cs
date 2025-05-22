namespace MCCS.ViewModels.Pages
{
    public class HardwareSettingPageViewModel:BaseViewModel
    {
        public HardwareSettingPageViewModel(IEventAggregator eventAggregator) : base(eventAggregator)
        {
        }

        public HardwareSettingPageViewModel(IEventAggregator eventAggregator, IDialogService dialogService) : base(eventAggregator, dialogService)
        {
        }
    }
}
