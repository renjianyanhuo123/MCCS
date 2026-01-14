using System.Globalization;
using System.Windows.Data;
using System.Windows.Media; 

namespace MCCS.Converters.ControlCommand
{
    public class BooleanToColorConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                return boolValue ? new SolidColorBrush(Color.FromArgb(255, 0, 188, 212)) : new SolidColorBrush(Colors.White); // Example colors, can be replaced with actual Color objects
            }
            return new SolidColorBrush(Colors.White); // Default color if value is not a boolean
        }
        
        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => Binding.DoNothing;
    }
}
