using Prism.Events;

namespace MCCS.ViewModels.Pages.ControlCommandPages
{
    public class ViewManualControlViewModel : BaseViewModel
    {
        public const string Tag = "ManualControl";

        private double _outputMaxValue = 100.0;
        private double _outputMinValue = 0.0;

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

        private double _changeStep = 1.0;
        public double ChangeStep
        {
            get => _changeStep;
            set => SetProperty(ref _changeStep, value);
        }
    }
}
