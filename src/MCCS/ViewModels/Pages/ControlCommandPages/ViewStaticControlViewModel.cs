using MCCS.Events.ControlCommand;
using MCCS.Models;
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
        public string ChannelId { get; set; }
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

        #region private method
        #endregion

        public override void OnNavigatedTo(NavigationContext navigationContext)
        {
            var success = navigationContext.Parameters.TryGetValue<StaticControlModel>("ControlModel", out var param);
            ChannelId = navigationContext.Parameters.GetValue<string>("ChannelId");
            if (success)
            { 
                SelectedControlUnitType = (int)param.UnitType;
                Speed = param.Speed;
                TargetValue = param.TargetValue;
            } 
        }

        public override void OnNavigatedFrom(NavigationContext navigationContext)
        {
            _eventAggregator.GetEvent<ControlParamEvent>().Publish(new ControlParamEventParam {
                ChannelId = ChannelId,
                ControlMode = ControlMode.Static,
                Param = new StaticControlModel
                {
                    ChannelId = ChannelId,
                    UnitType = (ControlUnitTypeEnum)SelectedControlUnitType,
                    Speed = Speed,
                    TargetValue = TargetValue
                }
            });
        }
    }
}
