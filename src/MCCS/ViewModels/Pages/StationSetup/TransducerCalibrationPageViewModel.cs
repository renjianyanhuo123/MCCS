namespace MCCS.ViewModels.Pages.StationSteup
{
    public sealed class TransducerCalibrationPageViewModel : BaseViewModel
    {
        public TransducerCalibrationPageViewModel(IEventAggregator eventAggregator) : base(eventAggregator)
        {
        }

        public TransducerCalibrationPageViewModel(IEventAggregator eventAggregator, IDialogService? dialogService) : base(eventAggregator, dialogService)
        {
        }
    }
}
