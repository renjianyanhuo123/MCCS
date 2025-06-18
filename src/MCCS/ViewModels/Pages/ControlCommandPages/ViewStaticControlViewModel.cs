using MCCS.Models.ControlCommand;
using MCCS.Services.ControlCommand;
using Prism.Events;

namespace MCCS.ViewModels.Pages.ControlCommandPages
{
    public class ViewStaticControlViewModel : BaseViewModel
    {
        public const string Tag = "StaticControl";
        private readonly ISharedStaticCommandService _sharedStaticCommandService;

        private int _selectedControlUnitType = 0;
        private double _speed = 0.0;

        public ViewStaticControlViewModel(
            ISharedStaticCommandService sharedStaticCommandService,
            IEventAggregator eventAggregator) : base(eventAggregator)
        {
            _sharedStaticCommandService = sharedStaticCommandService;
        }

        #region 页面属性
        public int SelectedControlUnitType
        {
            get => _selectedControlUnitType;
            set 
            {
                if (SetProperty(ref _selectedControlUnitType, value))
                {
                    _sharedStaticCommandService.UnitType = (ControlUnitTypeEnum)value;
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
                    _sharedStaticCommandService.Speed = value;
                }
            }
        }
        private double _targetValue = 0.0;
        public double TargetValue
        {
            get => _sharedStaticCommandService.TargetValue;
            set 
            {
                if (SetProperty(ref _targetValue, value))
                {
                    _sharedStaticCommandService.TargetValue = value;
                }
            }
        }
        #endregion
    }
}
