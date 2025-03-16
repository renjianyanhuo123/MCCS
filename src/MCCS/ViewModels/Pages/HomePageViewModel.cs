
namespace MCCS.ViewModels.Pages
{
    public class HomePageViewModel : BaseViewModel
    {
        public const string Tag = "HomePage";
        public HomePageViewModel(IEventAggregator eventAggregator, IDialogService dialogService) : base(eventAggregator, dialogService)
        {
        }
    }
}
