
namespace MCCS.ViewModels.Pages
{
    public class TestStartingPageViewModel(IEventAggregator eventAggregator, IDialogService dialogService)
        : BaseViewModel(eventAggregator, dialogService)
    {
        public const string Tag = "TestStartPage";

    }
}
