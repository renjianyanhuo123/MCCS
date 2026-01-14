namespace MCCS.Interface.Components.ViewModels.ControlCommandPages
{
    public class ViewFatigueControlViewModel : BindableBase
    {
        public const string Tag = "FatigueControl";   

        private int _controlUnitType = 0;
        private int _waveformType = 0;   
        private int _cycleTimes = 0;
        private int _cycleCount = 0;
         

        #region 页面属性  
        /// <summary>
        /// 控制模式
        /// 0-位移
        /// 1-力
        /// </summary>
        public int ControlUnitType
        { 
            get => _controlUnitType;
            set => SetProperty(ref _controlUnitType, value);
        }

        /// <summary>
        /// 波形类型
        /// 0=正弦，1=三角，2=方波
        /// </summary>
        public int WaveformType 
        { 
            get => _waveformType;
            set => SetProperty(ref _waveformType, value);
        }
        /// <summary>
        /// 频率
        /// </summary>
        private float _frequency = 0.0f;
        public float Frequency 
        { 
            get => _frequency;
            set => SetProperty(ref _frequency, value);
        }
        /// <summary>
        /// 幅值
        /// </summary>
        private float _amplitude = 0.0f;
        public float Amplitude 
        { 
            get => _amplitude;
            set => SetProperty(ref _amplitude, value);
        }
        /// <summary>
        /// 中值
        /// </summary>
        private float _median = 0.0f;
        public float Median 
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

        /// <summary>
        /// 相位补偿
        /// </summary>
        private float _compensatePhase = 0.0f;
        public float CompensatePhase
        {
            get => _compensatePhase;
            set => SetProperty(ref _compensatePhase, value);
        }

        /// <summary>
        /// 幅值补偿
        /// </summary>
        private float _compensateAmplitude = 0.0f; 
        public float CompensateAmplitude
        {
            get => _compensateAmplitude;
            set => SetProperty(ref _compensateAmplitude, value);
        }

        /// <summary>
        /// 是否正运行
        /// </summary>
        private bool _isRunning = false;
        public bool IsRunning
        {
            get => _isRunning;
            set => SetProperty(ref _isRunning, value);
        }

        #endregion
    }
}
