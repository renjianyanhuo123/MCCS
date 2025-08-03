using System.Windows;

namespace MCCS.UserControl.AttachedAttributes
{
    public class GroupBoxHelper
    {
        public static readonly DependencyProperty CheckStateProperty = DependencyProperty.RegisterAttached(
            "CheckState",
            typeof(bool?),
            typeof(GroupBoxHelper),
            new PropertyMetadata(false));

        public static bool? GetCheckState(DependencyObject obj)
        {
            return (bool?)obj.GetValue(CheckStateProperty);
        }

        public static void SetCheckState(DependencyObject obj, bool? value)
        {
            obj.SetValue(CheckStateProperty, value);
        }
    }
}
