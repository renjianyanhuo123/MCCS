using System.Globalization;
using System.Windows.Controls;
using System.Windows.Data;

namespace MCCS.UserControl.Converters
{
    public class ComboBoxItemToIntConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is int pageSize)
            {
                return $"{pageSize}{parameter}";
            }
            return "10条/页";
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            // 从ComboBoxItem转为int
            if (value is not ComboBoxItem comboBoxItem) return 0;
            var content = comboBoxItem.Content.ToString();
            if (content == null) return null;
            var suffix = parameter as string ?? "";

            if (!string.IsNullOrEmpty(suffix) && content.EndsWith(suffix))
            {
                content = content.Substring(0, content.Length - suffix.Length);
            }
            return int.TryParse(content, out var result) ? result : 0;
        }
    }
}
