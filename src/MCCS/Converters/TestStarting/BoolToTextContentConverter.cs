using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace MCCS.Converters.TestStarting
{
    public sealed class BoolToTextContentConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isStartedTest) 
            {
                return isStartedTest ? Application.Current.FindResource("StopTest") : Application.Current.FindResource("StartTest");
            }
            return "开始测试";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
