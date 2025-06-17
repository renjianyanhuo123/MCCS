using Prism.Events;

namespace MCCS.ViewModels.Pages.ControlCommandPages
{
    public class ViewManualControlViewModel : BaseViewModel
    {
        public const string Tag = "ManualControl";

        public ViewManualControlViewModel(IEventAggregator eventAggregator) : base(eventAggregator)
        {
        }
    }
}
