using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace MCCS.Converters.TestStarting
{
    public sealed class BoolToStyleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isStartedTest) 
            {
                return isStartedTest 
                    ? Application.Current.FindResource("MaterialDesignRaisedLightButton") 
                    : Application.Current.FindResource("MaterialDesignRaisedSecondaryDarkButton");
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
