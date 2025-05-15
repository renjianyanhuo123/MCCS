using System.Windows;
using System.Windows.Controls;

namespace MCCS.UserControl.AttachedAttributes
{
    public static class ComboBoxSuffix
    {
        public static readonly DependencyProperty SuffixProperty =
            DependencyProperty.RegisterAttached(
                "Suffix", 
                typeof(string), 
                typeof(ComboBoxSuffix), 
                new PropertyMetadata(string.Empty, OnSuffixChanged));

        public static string GetSuffix(DependencyObject obj)
        {
            return (string)obj.GetValue(SuffixProperty);
        }
        public static void SetSuffix(DependencyObject obj, string value)
        {
            obj.SetValue(SuffixProperty, value);
        }

        private static void OnSuffixChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not System.Windows.Controls.ComboBox comboBox || e.NewValue == null) return;
            comboBox.Loaded -= ComboBox_Loaded;
            comboBox.Loaded += ComboBox_Loaded;
        }

        private static void ComboBox_Loaded(object sender, RoutedEventArgs e)
        {
            if (sender is not System.Windows.Controls.ComboBox comboBox) return;
            var suffix = GetSuffix(comboBox);

            foreach (var item in comboBox.Items)
            {
                if (item is not ComboBoxItem { Tag: null } comboBoxItem) continue;
                // 原始内容存到 Tag
                comboBoxItem.Tag = comboBoxItem.Content;

                // 设置带后缀的显示
                comboBoxItem.Content = $"{comboBoxItem.Content}{suffix}";
            }
        }

    } 
}
