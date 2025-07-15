using System.Globalization;
using System.Windows.Data;
using System.Windows;

namespace MCCS.Converters.TestStarting
{
    public class BoolToGridLengthConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is bool boolean)
            {
                return boolean ? GridLength.Auto : new GridLength(0, GridUnitType.Pixel);
            }

            return new GridLength(0, GridUnitType.Pixel); // 默认 fallback 值
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is GridLength length)
            {
                return length.IsAuto || length.Value > 0;
            }

            return false;
        }
    }
}
