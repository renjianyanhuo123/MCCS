namespace MCCS.ViewModels.Pages.ControlCommandPages
{
    public class ViewStaticControlViewModel : BindableBase
    {
        public const string Tag = "StaticControl"; 

        private int _selectedControlUnitType = 0;
        private double _speed = 0.0; 
         
        #region 页面属性
        public int SelectedControlUnitType
        {
            get => _selectedControlUnitType;
            set => SetProperty(ref _selectedControlUnitType, value);
        }

        public double Speed 
        {
            get => _speed;
            set => SetProperty(ref _speed, value);
        }
        private double _targetValue = 0.0;
        public double TargetValue
        {
            get => _targetValue;
            set => SetProperty(ref _targetValue, value);
        }
        #endregion 
    }
}
