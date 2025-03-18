
namespace MCCS.ViewModels.Pages
{
    public class HomeTestOperationPageViewModel : BaseViewModel
    {
        public const string Tag = "HomeTestOperationPage";

        public HomeTestOperationPageViewModel(IEventAggregator eventAggregator, IDialogService dialogService) 
            : base(eventAggregator, dialogService)
        {
        }
    }
}
