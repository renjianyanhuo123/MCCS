using System.Globalization;
using System.Windows.Data;

using MCCS.Common.Resources.Models;

namespace MCCS.Common.Resources.Resources.Converters
{
    internal class NotificationTypeToIconText : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is NotificationType notificationType)
            {
                return notificationType switch
                {
                    NotificationType.Error => "CloseCircleOutline",
                    NotificationType.Info => "InformationSlabSymbol",
                    NotificationType.Success => "Check",
                    NotificationType.Warning => "ShieldAlertOutline",
                    _ => "InformationSlabSymbol"
                };
            } 
            return "InformationSlabSymbol";
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => throw new NotImplementedException();
    }
}
