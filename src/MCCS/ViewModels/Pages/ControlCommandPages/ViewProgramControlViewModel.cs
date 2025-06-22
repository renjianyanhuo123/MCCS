using MCCS.Events.ControlCommand;
using MCCS.Models;
using MCCS.Models.ControlCommand;

namespace MCCS.ViewModels.Pages.ControlCommandPages
{
    public class ViewProgramControlViewModel : BaseViewModel
    {
        public const string Tag = "ProgramControl"; 
        private string? _filePath = string.Empty; 

        public ViewProgramControlViewModel(IEventAggregator eventAggregator) : base(eventAggregator)
        {
        }
        public string? FilePath 
        {
            get => _filePath;
            set 
            {
                if (SetProperty(ref _filePath, value)) 
                {
                    SendUpdateEvent();
                }
            }
        } 
        public override void OnNavigatedTo(NavigationContext navigationContext)
        {
            var success = navigationContext.Parameters.TryGetValue<ProgramControlModel>("ControlModel", out var param); 
            if (success) 
            {
                FilePath = param.FilePath;
            }
        } 

        private void SendUpdateEvent()
        {
            _eventAggregator.GetEvent<ControlParamEvent>().Publish(new ControlParamEventParam
            {
                Param = new ProgramControlModel
                {
                    FilePath = FilePath,
                }
            });
        }
    }
}
