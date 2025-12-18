using System.Windows;

namespace MCCS.UserControl.DynamicGrid
{
    public class GridSizeDefinitionModel : BindingBase
    {
        private double _value = 0.0;
        public double Value { get => _value; set => SetProperty(ref _value, value); }

        private GridUnitType _unitType = GridUnitType.Star;
        public GridUnitType UnitType { get => _unitType; set => SetProperty(ref _unitType, value); }
    }
}
