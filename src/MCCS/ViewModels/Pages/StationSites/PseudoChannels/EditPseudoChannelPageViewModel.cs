namespace MCCS.ViewModels.Pages.StationSites.PseudoChannels
{
    public sealed class EditPseudoChannelPageViewModel : BaseViewModel 
    {
        public const string Tag = "EditPseudoChannelPage";

        private long _stationId = -1;

        public EditPseudoChannelPageViewModel(IEventAggregator eventAggregator) : base(eventAggregator)
        {
        }
    }
}
