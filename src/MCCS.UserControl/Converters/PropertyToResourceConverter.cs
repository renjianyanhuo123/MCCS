using System.Globalization;
using System.Windows.Data;

namespace MCCS.UserControl.Converters;

public class PropertyToResourceConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values[0] is not bool isSelected || parameter is not string propertyName) return values[0];
        var resourceOwner = values[1] as System.Windows.Controls.UserControl ?? throw new NullReferenceException();
        return isSelected ?
            resourceOwner.FindResource(propertyName == "PageTextBlock" ? "SelectedPageTextBlockStyle" : "SelectedBorderStyle") :
            resourceOwner.FindResource(propertyName == "PageTextBlock" ? "PageTextBlockStyle" : "PageBorderStyle");
    } 

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}