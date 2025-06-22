using MCCS.Events.ControlCommand;
using MCCS.Models;
using MCCS.Models.ControlCommand;

namespace MCCS.ViewModels.Pages.ControlCommandPages
{
    public class ViewFatigueControlViewModel : BaseViewModel
    {
        public const string Tag = "FatigueControl";   

        private int _controlUnitType = 0;
        private int _waveformType = 0;
        private double _frequency = 0.0;
        private double _amplitude = 0.0;
        private double _median = 0.0;
        private int _cycleTimes = 0;
        private int _cycleCount = 0; 

        public ViewFatigueControlViewModel(IEventAggregator eventAggregator) : base(eventAggregator)
        { 
        } 

        #region 页面属性 
        /// <summary>
        /// 控制模式
        /// </summary>
        public int ControlUnitType
        { 
            get => _controlUnitType;
            set
            {
                if (SetProperty(ref _controlUnitType, value)) SendUpdateEvent();
            }
        }

        /// <summary>
        /// 波形类型
        /// </summary>
        public int WaveformType 
        { 
            get => _waveformType;
            set
            {
                if (SetProperty(ref _waveformType, value)) SendUpdateEvent();
            }
        }
        /// <summary>
        /// 频率
        /// </summary>
        public double Frequency 
        { 
            get => _frequency;
            set
            {
                if (SetProperty(ref _frequency, value)) SendUpdateEvent();
            }
        }
        /// <summary>
        /// 幅值
        /// </summary>
        public double Amplitude 
        { 
            get => _amplitude;
            set
            {
                if (SetProperty(ref _amplitude, value)) SendUpdateEvent();
            }
        }
        /// <summary>
        /// 中值
        /// </summary>
        public double Median 
        { 
            get => _median;
            set
            {
                if (SetProperty(ref _median, value)) SendUpdateEvent();
            }
        }
        /// <summary>
        /// 循环次数
        /// </summary>
        public int CycleTimes 
        { 
            get => _cycleTimes;
            set
            {
                if (SetProperty(ref _cycleTimes, value)) SendUpdateEvent();
            }
        }
        /// <summary>
        /// 循环计数
        /// </summary>
        public int CycleCount
        { 
            get => _cycleCount;
            set 
            {
                if (SetProperty(ref _cycleCount, value)) SendUpdateEvent();
            }
        }
        #endregion

        public override void OnNavigatedTo(NavigationContext navigationContext)
        {
            var success = navigationContext.Parameters.TryGetValue<FatigueControlModel>("ControlModel", out var param); 
            if (success)
            { 
                ControlUnitType = (int)param.ControlUnitType;
                WaveformType = (int)param.WaveformType;
                Frequency = param.Frequency;
                Amplitude = param.Amplitude;
                Median = param.Median;
                CycleTimes = param.CycleTimes;
                CycleCount = param.CycleCount;
            }
            
        } 

        private void SendUpdateEvent() 
        {
            _eventAggregator.GetEvent<ControlParamEvent>().Publish(new ControlParamEventParam
            {
                Param = new FatigueControlModel
                {
                    ControlUnitType = (ControlUnitTypeEnum)ControlUnitType,
                    WaveformType = (WaveformTypeEnum)WaveformType,
                    Frequency = Frequency,
                    Amplitude = Amplitude,
                    Median = Median,
                    CycleTimes = CycleTimes,
                    CycleCount = CycleCount
                }
            });
        }
    }
}
