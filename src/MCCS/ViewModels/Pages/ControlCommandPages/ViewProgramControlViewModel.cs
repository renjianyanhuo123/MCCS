
using MCCS.ViewModels;
using Prism.Events;

namespace MCCS.ViewModels.Pages.ControlCommandPages
{
    public class ViewProgramControlViewModel : BaseViewModel
    {
        public const string Tag = "ProgramControl";
        public ViewProgramControlViewModel(IEventAggregator eventAggregator) : base(eventAggregator)
        {
        }
    }
}
