using MCCS.Models.ControlCommand;
using System.Collections.ObjectModel;

namespace MCCS.ViewModels.Pages.ControlCommandPages
{
    public class ViewProgramControlViewModel : BindableBase
    {
        public const string Tag = "ProgramControl"; 
        private string? _filePath = string.Empty;

        public ViewProgramControlViewModel(IEnumerable<ControlChannelBindModel> channels)
        {
            foreach (var channel in channels)
            {
                Channels.Add(channel);
            }
        }

        public ObservableCollection<ControlChannelBindModel> Channels { get; private set; } = [];

        public string? FilePath 
        {
            get => _filePath;
            set => SetProperty(ref _filePath, value);
        }
    }
}
