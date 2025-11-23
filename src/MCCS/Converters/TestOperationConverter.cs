using MaterialDesignThemes.Wpf;
using System.Globalization;
using System.Windows.Data;
using MCCS.Infrastructure.Models.TestInfo;

namespace MCCS.Converters
{
    public class TestOperationConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is TestStatus status) 
            {
                return status switch
                {
                    TestStatus.Stop => PackIconKind.Play,
                    TestStatus.Processing => PackIconKind.Stop,
                    TestStatus.Success or TestStatus.Failed => PackIconKind.EyeSettings,
                    _ => (object)PackIconKind.EyeSettings,
                };
            }
            return PackIconKind.EyeSettings;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
