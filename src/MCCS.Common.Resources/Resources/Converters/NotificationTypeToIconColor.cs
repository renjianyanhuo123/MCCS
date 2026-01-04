using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

using MCCS.Common.Resources.Models;

namespace MCCS.Common.Resources.Resources.Converters
{
    internal class NotificationTypeToIconColor : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is NotificationType notificationType)
            {
                return notificationType switch
                {
                    NotificationType.Error => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F44336")),
                    NotificationType.Info => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2196F3")),
                    NotificationType.Success => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#4CAF50")),
                    NotificationType.Warning => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF9800")),
                    _ => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2196F3"))
                };
            }
            return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2196F3"));
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => throw new NotImplementedException();
    }
}
