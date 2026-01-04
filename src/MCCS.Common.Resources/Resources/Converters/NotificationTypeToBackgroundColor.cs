using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

using MCCS.Common.Resources.Models;

namespace MCCS.Common.Resources.Resources.Converters
{
    internal class NotificationTypeToBackgroundColor : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is NotificationType notificationType)
            {
                return notificationType switch
                {
                    NotificationType.Error => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFEBEE")),
                    NotificationType.Info => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E3F2FD")),
                    NotificationType.Success => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E8F5E8")),
                    NotificationType.Warning => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFF3E0")),
                    _ => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E3F2FD"))
                };
            }
            return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E3F2FD"));
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => throw new NotImplementedException();
    }
}
