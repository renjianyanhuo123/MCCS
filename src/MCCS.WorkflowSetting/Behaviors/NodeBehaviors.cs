using System.Windows.Input;
using System.Windows;

namespace MCCS.WorkflowSetting.Behaviors
{
    public static class NodeBehaviors
    {
        public static readonly DependencyProperty NodeClickCommandProperty =
            DependencyProperty.RegisterAttached(
                "NodeClickCommand",
                typeof(ICommand),
                typeof(NodeBehaviors),
                new PropertyMetadata(null, OnNodeClickCommandChanged));

        public static void SetNodeClickCommand(DependencyObject obj, ICommand value)
        {
            obj.SetValue(NodeClickCommandProperty, value);
        }

        public static ICommand GetNodeClickCommand(DependencyObject obj)
        {
            return (ICommand)obj.GetValue(NodeClickCommandProperty);
        }

        private static void OnNodeClickCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is UIElement element)
            {
                element.MouseLeftButtonDown -= Element_MouseLeftButtonDown;
                if (e.NewValue is ICommand)
                {
                    element.MouseLeftButtonDown += Element_MouseLeftButtonDown;
                }
            }
        }

        private static void Element_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var element = sender as FrameworkElement;
            var command = GetNodeClickCommand(element);
            if (command?.CanExecute(element.DataContext) == true)
            {
                command.Execute(element.DataContext);
                e.Handled = true;
            }
        }
    }
}
