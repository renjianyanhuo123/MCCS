using System.Collections.ObjectModel;

using MCCS.Interface.Components.Models;

namespace MCCS.Interface.Components.ViewModels.ControlCommandPages
{
    public class ViewProgramControlViewModel : BindableBase
    {
        public const string Tag = "ProgramControl"; 
        private string? _filePath = string.Empty;

        public ViewProgramControlViewModel()
        { 
        }

        public ObservableCollection<ControlChannelBindModel> Channels { get; private set; } = [];

        public string? FilePath 
        {
            get => _filePath;
            set => SetProperty(ref _filePath, value);
        }
    }
}
