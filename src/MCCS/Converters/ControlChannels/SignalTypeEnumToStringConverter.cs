using System.Globalization;
using System.Windows.Data;
using MCCS.Core.Models.StationSites;

namespace MCCS.Converters.ControlChannels
{
    public class SignalTypeEnumToStringConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is SignalTypeEnum signalType)
            {
                return signalType switch
                {
                    SignalTypeEnum.Force => "力反馈",
                    SignalTypeEnum.Position => "位移反馈",
                    SignalTypeEnum.Output => "伺服阀输出",
                    _ => string.Empty,
                };
            }
            return string.Empty;
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
