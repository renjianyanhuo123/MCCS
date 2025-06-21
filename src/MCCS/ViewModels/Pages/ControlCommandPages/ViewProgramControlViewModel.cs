
using MCCS.ViewModels;
using Prism.Events;

namespace MCCS.ViewModels.Pages.ControlCommandPages
{
    public class ViewProgramControlViewModel : BaseViewModel
    {
        public const string Tag = "ProgramControl";

        private string _filePath = string.Empty;

        public ViewProgramControlViewModel(IEventAggregator eventAggregator) : base(eventAggregator)
        {
        }

        public string FilePath 
        {
            get => _filePath;
            set => SetProperty(ref _filePath, value);
        }
    }
}
