using MCCS.Events.ControlCommand;
using MCCS.Models.ControlCommand;

namespace MCCS.ViewModels.Pages.ControlCommandPages
{
    public class ViewStaticControlViewModel : BaseViewModel
    {
        public const string Tag = "StaticControl";

        private readonly IEventAggregator _eventAggregator;

        private int _selectedControlUnitType = 0;
        private double _speed = 0.0;

        private StaticControlModel _staticModel = new();

        public ViewStaticControlViewModel( 
            IEventAggregator eventAggregator) : base(eventAggregator)
        {
            _eventAggregator = eventAggregator;
        }

        #region 页面属性
        public int SelectedControlUnitType
        {
            get => _selectedControlUnitType;
            set 
            {
                if (SetProperty(ref _selectedControlUnitType, value))
                {
                    UpdateModel();
                }
            }
        }

        public double Speed 
        {
            get => _speed;
            set 
            {
                if (SetProperty(ref _speed, value)) 
                {
                    UpdateModel();
                }
            }
        }
        private double _targetValue = 0.0;
        public double TargetValue
        {
            get => _targetValue;
            set 
            {
                if (SetProperty(ref _targetValue, value))
                {
                    UpdateModel();
                }
            }
        }
        #endregion

        #region private method
        private void UpdateModel() 
        {
            _eventAggregator.GetEvent<ControlParamEvent>().Publish(
                        new ControlParamEventParam
                        {
                            ControlMode = Models.ControlMode.Static,
                            Param = new StaticControlModel 
                            {
                                UnitType = (ControlUnitTypeEnum)SelectedControlUnitType,
                                Speed = Speed,
                                TargetValue = TargetValue
                            }
                        });
            
        }
        #endregion

        public override void OnNavigatedTo(NavigationContext navigationContext)
        {
            base.OnNavigatedTo(navigationContext);
        }
    }
}
