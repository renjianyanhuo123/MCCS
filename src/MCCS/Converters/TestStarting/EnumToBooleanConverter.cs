using System.Globalization;
using System.Windows.Data;

namespace MCCS.Converters.TestStarting
{
    public sealed class EnumToBooleanConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            return value != null && value.ToString() == parameter?.ToString();
        }

        public object ConvertBack(object value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value == null) throw new ArgumentNullException(nameof(value));
            return (bool)value ? Enum.Parse(targetType, parameter?.ToString() ?? string.Empty) : Binding.DoNothing;
        }
    }
}
