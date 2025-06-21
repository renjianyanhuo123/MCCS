
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
        public string ChannelId { get; set; }
        public string? FilePath 
        {
            get => _filePath;
            set => SetProperty(ref _filePath, value);
        }

        public override void OnNavigatedTo(NavigationContext navigationContext)
        {
            var success = navigationContext.Parameters.TryGetValue<ProgramControlModel>("ControlModel", out var param);
            ChannelId = navigationContext.Parameters.GetValue<string>("ChannelId");
            if (success) 
            {
                FilePath = param.FilePath;
            }
        }

        public override void OnNavigatedFrom(NavigationContext navigationContext)
        {
            _eventAggregator.GetEvent<ControlParamEvent>().Publish(new ControlParamEventParam 
            {
                ChannelId = ChannelId,
                ControlMode = ControlMode.Programmable,
                Param = new ProgramControlModel 
                {
                    ChannelId = ChannelId,
                    FilePath = FilePath,
                }
            });
        }
    }
}
