using MaterialDesignThemes.Wpf;
using System.Globalization;
using System.Windows.Data;

namespace MCCS.Converters.TestStarting
{
    public sealed class BooleanToPauseAndContinueIconConverter:IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is bool isPaused)
            {
                return isPaused ? new PackIcon
                {
                    Width = 32,
                    Height = 32,
                    Kind = PackIconKind.Play
                } : new PackIcon
                {
                    Width = 32,
                    Height = 32,
                    Kind = PackIconKind.Pause
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
