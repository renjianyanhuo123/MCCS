namespace MCCS.ViewModels.Pages.ControlCommandPages
{
    public class ViewProgramControlViewModel : BindableBase
    {
        public const string Tag = "ProgramControl"; 
        private string? _filePath = string.Empty; 
         
        public string? FilePath 
        {
            get => _filePath;
            set => SetProperty(ref _filePath, value);
        }
    }
}
