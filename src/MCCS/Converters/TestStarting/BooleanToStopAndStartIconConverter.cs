using System.Globalization;
using System.Windows.Data;
using MaterialDesignThemes.Wpf;

namespace MCCS.Converters.TestStarting
{
    public sealed class BooleanToStopAndStartIconConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is bool isStartedTest)
            {
                return isStartedTest ? new PackIcon
                {
                    Width = 32,
                    Height = 32,
                    Kind = PackIconKind.Stop
                } : new PackIcon
                {
                    Width = 32,
                    Height = 32,
                    Kind = PackIconKind.Play
                };
            }

            return new PackIcon
            {
                Width = 32,
                Height = 32,
                Kind = PackIconKind.Play
            };
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
