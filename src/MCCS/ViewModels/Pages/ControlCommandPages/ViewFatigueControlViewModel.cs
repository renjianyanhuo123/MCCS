using MCCS.Models;
using MCCS.Models.ControlCommand;

namespace MCCS.ViewModels.Pages.ControlCommandPages
{
    public class ViewFatigueControlViewModel : BaseViewModel
    {
        public const string Tag = "FatigueControl";
        private readonly IEventAggregator _eventAggregator;

        private int _controlMode = 0;
        private int _waveformType = 0;
        private double _frequency = 0.0;
        private double _amplitude = 0.0;
        private double _median = 0.0;
        private int _cycleTimes = 0;
        private int _cycleCount = 0;

        public ViewFatigueControlViewModel(IEventAggregator eventAggregator) : base(eventAggregator)
        {
            _eventAggregator = eventAggregator;
        }

        #region 页面属性 
        /// <summary>
        /// 控制模式
        /// </summary>
        public int ControlMode 
        { 
            get => _controlMode; 
            set => SetProperty(ref _controlMode, value); 
        }

        /// <summary>
        /// 波形类型
        /// </summary>
        public int WaveformType 
        { 
            get => _waveformType; 
            set => SetProperty(ref _waveformType, value); 
        }
        /// <summary>
        /// 频率
        /// </summary>
        public double Frequency 
        { 
            get => _frequency; 
            set => SetProperty(ref _frequency, value); 
        }
        /// <summary>
        /// 幅值
        /// </summary>
        public double Amplitude 
        { 
            get => _amplitude; 
            set => SetProperty(ref _amplitude, value); 
        }
        /// <summary>
        /// 中值
        /// </summary>
        public double Median 
        { 
            get => _median; 
            set => SetProperty(ref _median, value); 
        }
        /// <summary>
        /// 循环次数
        /// </summary>
        public int CycleTimes 
        { 
            get => _cycleTimes; 
            set => SetProperty(ref _cycleTimes, value); 
        }
        /// <summary>
        /// 循环计数
        /// </summary>
        public int CycleCount
        { 
            get => _cycleCount; 
            set => SetProperty(ref _cycleCount, value); 
        }
        #endregion
    }
}
