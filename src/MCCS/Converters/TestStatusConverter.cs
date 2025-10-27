using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using MCCS.Core.Models.TestInfo;

namespace MCCS.Converters
{
    public class TestStatusConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is TestStatus status)
            {
                switch (status)
                {
                    case TestStatus.Failed:
                        return Brushes.Red;
                    case TestStatus.Stop:
                    case TestStatus.Processing:
                        return Brushes.Blue;
                    case TestStatus.Success:
                        return Brushes.Green;
                    default:
                        break;
                }
            }
            return Brushes.Gray;
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
