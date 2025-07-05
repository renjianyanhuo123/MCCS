using MCCS.Models.ControlCommand;
using System.Globalization;
using System.Windows.Data;

namespace MCCS.Converters.ControlCommand
{
    public class EnumToSuffixTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int unitType) 
            {
                var unit = (ControlUnitTypeEnum)unitType;
                if (parameter is not string addText) 
                {
                    addText = string.Empty;
                }
                return unit switch
                {
                    ControlUnitTypeEnum.Force => $"kN{addText}",
                    _ => $"mm{addText}",
                };
            }
            return $"mm";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
