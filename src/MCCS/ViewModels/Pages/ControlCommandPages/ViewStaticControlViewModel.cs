using MCCS.Events.ControlCommand;
using MCCS.Models.ControlCommand;

namespace MCCS.ViewModels.Pages.ControlCommandPages
{
    public class ViewStaticControlViewModel : BaseViewModel
    {
        public const string Tag = "StaticControl"; 

        private int _selectedControlUnitType = 0;
        private double _speed = 0.0; 

        public ViewStaticControlViewModel( 
            IEventAggregator eventAggregator) : base(eventAggregator)
        {
        } 
        #region 页面属性
        public int SelectedControlUnitType
        {
            get => _selectedControlUnitType;
            set
            {
                if (SetProperty(ref _selectedControlUnitType, value))
                {
                    SendUpdateEvent();
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
                    SendUpdateEvent();
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
                    SendUpdateEvent();
                }
            }
        }
        #endregion

        #region private method
        private void SendUpdateEvent()
        { 
            _eventAggregator.GetEvent<ControlParamEvent>().Publish(new ControlParamEventParam
            { 
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
            var success = navigationContext.Parameters.TryGetValue<StaticControlModel>("ControlModel", out var param);
            if (success)
            { 
                SelectedControlUnitType = (int)param.UnitType;
                Speed = param.Speed;
                TargetValue = param.TargetValue;
            } 
        } 
    }
}
