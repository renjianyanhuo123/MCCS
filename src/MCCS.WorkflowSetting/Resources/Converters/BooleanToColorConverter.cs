using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace MCCS.WorkflowSetting.Resources.Converters
{
    public class BooleanToColorConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is bool flag)
            {
                return flag ? Brushes.White : (Brush)new BrushConverter().ConvertFromString("#1E88E5")!;
            }

            return Brushes.Blue; // 默认值
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => throw new NotImplementedException();
    }
}
