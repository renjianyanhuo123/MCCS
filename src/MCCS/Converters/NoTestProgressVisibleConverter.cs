using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace MCCS.Converters
{
    public class NoTestProgressVisibleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isLoaded)
            {
                return isLoaded ? Visibility.Visible : Visibility.Hidden;
            }
            return Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
