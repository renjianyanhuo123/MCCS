using Microsoft.Xaml.Behaviors;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace MCCS.UserControl.Behaviors
{
    public class EnterKeyCommandBehavior : Behavior<TextBox>
    {
        public static readonly DependencyProperty CommandProperty =
            DependencyProperty.Register(nameof(Command), typeof(ICommand), typeof(EnterKeyCommandBehavior));

        public static readonly DependencyProperty CommandParameterProperty =
            DependencyProperty.Register(nameof(CommandParameter), typeof(object), typeof(EnterKeyCommandBehavior));

        public ICommand Command
        {
            get => (ICommand)GetValue(CommandProperty);
            set => SetValue(CommandProperty, value);
        }

        public object CommandParameter
        {
            get => GetValue(CommandParameterProperty);
            set => SetValue(CommandParameterProperty, value);
        }

        private bool _hasFocus = false;

        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.GotFocus += OnGotFocus;
            AssociatedObject.LostFocus += OnLostFocus;
            AssociatedObject.KeyDown += OnKeyDown;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            AssociatedObject.GotFocus -= OnGotFocus;
            AssociatedObject.LostFocus -= OnLostFocus;
            AssociatedObject.KeyDown -= OnKeyDown;
        }

        private void OnGotFocus(object sender, RoutedEventArgs e)
        {
            _hasFocus = true;
        }

        private void OnLostFocus(object sender, RoutedEventArgs e)
        {
            _hasFocus = false;
        }

        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            if (_hasFocus && e.Key == Key.Enter && Command.CanExecute(CommandParameter) == true)
            {
                Command.Execute(CommandParameter);
            }
        }
    }
}
