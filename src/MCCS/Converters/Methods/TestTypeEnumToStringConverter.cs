using System.Globalization;
using System.Windows.Data; 
using MCCS.Infrastructure.Models.MethodManager;

namespace MCCS.Converters.Methods
{
    public sealed class TestTypeEnumToStringConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is TestTypeEnum temp)
            {
                return temp switch
                {
                    TestTypeEnum.Dynamic => "动态",
                    TestTypeEnum.Static => "静态",
                    _ => "未知"
                };
            }
            return null;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
