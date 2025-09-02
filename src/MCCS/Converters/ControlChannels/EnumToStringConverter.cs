using MCCS.Core.Models.StationSites;
using System.Globalization;
using System.Windows.Data;

namespace MCCS.Converters.ControlChannels
{
    public class EnumToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is ControlChannelModeTypeEnum controlMode)
            {
                return controlMode switch
                {
                    ControlChannelModeTypeEnum.FCSLoop => "力/位移闭环控制",
                    ControlChannelModeTypeEnum.OpenLoop => "开环控制",
                    _ => string.Empty
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
