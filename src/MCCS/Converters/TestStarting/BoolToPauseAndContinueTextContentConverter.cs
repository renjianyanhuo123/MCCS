using System.Globalization;
using System.Windows.Data;

namespace MCCS.Converters.TestStarting
{
    public sealed class BoolToPauseAndContinueTextContentConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is bool isPausedTest)
            {
                return isPausedTest ? "继续试验" : "暂停试验";
            }
            return "继续试验";
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
