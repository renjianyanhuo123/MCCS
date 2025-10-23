using System.Globalization;
using System.Windows;
using System.Windows.Data;
using MCCS.Core.Models.Model3D;

namespace MCCS.Converters.ModelSetttings
{
    public class BillboardTypeToVisilityConverter:IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is int billboardType)
            {
                var typeEnum = (BillboardTypeEnum)billboardType;
                return typeEnum switch
                {
                    BillboardTypeEnum.DataShow => Visibility.Visible,
                    BillboardTypeEnum.ValveStatus => Visibility.Collapsed,
                    _ => Visibility.Visible
                };
            }

            return Visibility.Visible;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
