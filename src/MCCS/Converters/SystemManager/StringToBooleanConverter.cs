using System.Globalization;
using System.Windows.Data;

namespace MCCS.Converters.SystemManager
{
    public sealed class StringToBooleanConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is string pageId && parameter is string checkPageId)
            {
                return pageId == checkPageId;
            }
            return true;
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            // 当RadioButton被选中时，返回对应的页面ID
            if (value is true && parameter is string pageId)
            {
                return pageId;
            }

            // 如果没有被选中或参数无效，返回DoNothing避免更新源
            return Binding.DoNothing;
        }
    }
}
