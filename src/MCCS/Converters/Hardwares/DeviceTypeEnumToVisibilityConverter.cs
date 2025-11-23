using System.Globalization;
using System.Windows;
using System.Windows.Data; 
using MCCS.Infrastructure.Models.Devices;

namespace MCCS.Converters.Hardwares
{
    public class DeviceTypeEnumToVisibilityConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is DeviceTypeEnum deviceType)
            {
                return deviceType switch
                {
                    DeviceTypeEnum.Controller => Visibility.Visible,
                    _ => Visibility.Collapsed
                };
            }

            return Visibility.Visible;
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
