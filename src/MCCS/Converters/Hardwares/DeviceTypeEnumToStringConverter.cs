using System.Globalization;
using System.Windows.Data;
using MCCS.Infrastructure.Models.Devices;

namespace MCCS.Converters.Hardwares
{
    public class DeviceTypeEnumToStringConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is DeviceTypeEnum deviceType)
            {
                switch (deviceType)
                {
                    case DeviceTypeEnum.Sensor:
                        return "传感器";
                    case DeviceTypeEnum.Actuator:
                        return "作动器";
                    case DeviceTypeEnum.Controller:
                        return "控制器";
                    case DeviceTypeEnum.Gateway:
                        return "网关";
                    case DeviceTypeEnum.Display:
                        return "服务器";
                    case DeviceTypeEnum.Other:
                        return "其他";
                    case DeviceTypeEnum.Unknown:
                    default:
                        return "未知";
                }
            }
            return string.Empty;
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
