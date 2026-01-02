using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace MCCS.Common.Resources.ExtensionAttributes
{
    public static class FocusHelper
    {
        public static readonly DependencyProperty AutoFocusProperty =
            DependencyProperty.RegisterAttached(
                "AutoFocus",
                typeof(bool),
                typeof(FocusHelper),
                new PropertyMetadata(false, OnAutoFocusChanged));

        public static bool GetAutoFocus(DependencyObject obj)
            => (bool)obj.GetValue(AutoFocusProperty);

        public static void SetAutoFocus(DependencyObject obj, bool value)
            => obj.SetValue(AutoFocusProperty, value);

        private static void OnAutoFocusChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is UIElement element && (bool)e.NewValue)
            {
                // 必须延迟到布局完成后
                element.Dispatcher.BeginInvoke(new Action(() =>
                {
                    element.Focus();
                    Keyboard.Focus(element);
                }), DispatcherPriority.Input);
            }
        }
    }

}
