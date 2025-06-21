using MCCS.Events.ControlCommand;
using MCCS.Models;
using MCCS.Models.ControlCommand;

namespace MCCS.ViewModels.Pages.ControlCommandPages
{
    public class ViewManualControlViewModel : BaseViewModel
    {
        public const string Tag = "ManualControl";

        private double _outputMaxValue = 100.0;
        private double _outputMinValue = 0.0;
        private double _outPutValue = 0.0;  

        public ViewManualControlViewModel(IEventAggregator eventAggregator) : base(eventAggregator)
        { 
        }

        public double OutputMaxValue
        {
            get => _outputMaxValue;
            set 
            {
                if (SetProperty(ref _outputMaxValue, value))
                {
                    ChangeStep = Math.Abs(OutputMaxValue - OutputMinValue) / 20.0; // 计算步长为最大值和最小值之差的20之一 
                }
            }
        }

        public double OutputMinValue
        {
            get => _outputMinValue;
            set 
            {
                if (SetProperty(ref _outputMinValue, value)) 
                {
                    ChangeStep = Math.Abs(OutputMaxValue - OutputMinValue) / 20.0; // 计算步长为最大值和最小值之差的20之一  
                }
            }
        }
        public string ChannelId { get; set; }
        private double _changeStep = 1.0;
        public double ChangeStep
        {
            get => _changeStep;
            set => SetProperty(ref _changeStep, value);
        }

        public double OutPutValue
        {
            get => _outPutValue;
            set => SetProperty(ref _outPutValue, value);
        }

        public override void OnNavigatedTo(NavigationContext navigationContext)
        {
            var success = navigationContext.Parameters.TryGetValue<ManualControlModel>("ControlModel", out var res);
            ChannelId = navigationContext.Parameters.GetValue<string>("ChannelId"); 
            if (success)
            {
                OutputMaxValue = res.MaxValue;
                OutputMinValue = res.MinValue;
                OutPutValue = res.OutputValue;
            } 
        }

        public override void OnNavigatedFrom(NavigationContext navigationContext)
        {
            _eventAggregator.GetEvent<ControlParamEvent>().Publish(new ControlParamEventParam 
            {
                ChannelId = ChannelId,
                ControlMode = ControlMode.Manual,
                Param = new ManualControlModel
                {
                    ChannelId = ChannelId,
                    MaxValue = OutputMaxValue,
                    MinValue = OutputMinValue,
                    OutputValue = OutPutValue
                }
            });
        }
    }
}
