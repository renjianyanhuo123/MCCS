using MCCS.Models.Stations.Model3DSettings;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace MCCS.Converters.ModelSetttings
{
    public sealed class BillBoradInfoIsNullToVisilityConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is BindedChannelModelBillboardTextInfo selectedFile) return Visibility.Visible;
            return Visibility.Collapsed;
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
