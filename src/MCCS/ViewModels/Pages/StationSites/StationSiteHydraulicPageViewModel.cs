using MCCS.Common.Resources.ViewModels;

namespace MCCS.ViewModels.Pages.StationSites
{
    public sealed class StationSiteHydraulicPageViewModel : BaseViewModel
    {
        public const string Tag = "StationSiteHydraulic";

        public StationSiteHydraulicPageViewModel(IEventAggregator eventAggregator) : base(eventAggregator)
        {
        }
    }
}
