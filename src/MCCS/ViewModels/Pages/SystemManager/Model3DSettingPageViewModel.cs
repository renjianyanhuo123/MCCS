namespace MCCS.ViewModels.Pages.SystemManager
{
    public class Model3DSettingPageViewModel : BaseViewModel
    {
        public const string Tag = "Model3DSetting";

        public Model3DSettingPageViewModel(IEventAggregator eventAggregator) : base(eventAggregator)
        {
        }

        public Model3DSettingPageViewModel(IEventAggregator eventAggregator, IDialogService? dialogService) : base(eventAggregator, dialogService)
        {
        }


    }
}
