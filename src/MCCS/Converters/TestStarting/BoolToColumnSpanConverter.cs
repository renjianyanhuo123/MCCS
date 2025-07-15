using System.Globalization;
using System.Windows.Data;

namespace MCCS.Converters.TestStarting
{
    public class BoolToColumnSpanConverter:IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                return boolValue ? 1 : 3; // 如果显示控制器，跨度为1；否则跨度为3（占据整个Grid）
            }
            return 1;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
