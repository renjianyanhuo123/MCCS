namespace MCCS.Models.ControlCommand
{
    public class ChannelInfo 
    {
        public required string ChannelId { get; set; }
        public required string ChannelName { get; set; } 
    }

    public class ProcessShowModel : BindableBase
    {
        private double _processValue;

        public required List<ChannelInfo> ChannelInfo { get; set; }

        public double ProcessValue 
        { 
            get => _processValue;
            set => SetProperty(ref _processValue, value); 
        }

        public ControlTypeEnum ControlType { get; set; }

        public ControlMode ControlMode { get; set; }
    }
}
