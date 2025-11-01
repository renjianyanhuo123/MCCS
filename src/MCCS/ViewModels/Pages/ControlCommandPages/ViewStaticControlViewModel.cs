namespace MCCS.ViewModels.Pages.ControlCommandPages
{
    public class ViewStaticControlViewModel : BindableBase
    {
        public const string Tag = "StaticControl"; 
         
        #region 页面属性
        /// <summary>
        /// 0-位移
        /// 1-力
        /// </summary>
        private int _selectedControlUnitType = 0;
        public int SelectedControlUnitType
        {
            get => _selectedControlUnitType;
            set
            {
                if (SetProperty(ref _selectedControlUnitType, value))
                {
                    SelectedTargetUnitType = _selectedControlUnitType;
                }
            }
        }

        /// <summary>
        /// 0 - mm
        /// 1 - kN
        /// </summary>
        private int _selectedTargetUnitType; 
        public int SelectedTargetUnitType
        {
            get => _selectedTargetUnitType;
            set => SetProperty(ref _selectedTargetUnitType, value);
        }

        private float _speed = 0.0f;
        public float Speed 
        {
            get => _speed;
            set => SetProperty(ref _speed, value);
        }
        private float _targetValue = 0.0f;
        public float TargetValue
        {
            get => _targetValue;
            set => SetProperty(ref _targetValue, value);
        }
        #endregion 
    }
}
