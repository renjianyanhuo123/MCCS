using HelixToolkit.Wpf.SharpDX;
using Microsoft.Xaml.Behaviors;
using System.Windows.Input;
using System.Windows;

namespace MCCS.Behaviors
{
    public class MouseMoveHitTestBehavior : Behavior<Viewport3DX>
    {
        public static readonly DependencyProperty HitTestCommandProperty =
            DependencyProperty.Register(nameof(HitTestCommand), typeof(ICommand), typeof(MouseMoveHitTestBehavior), new PropertyMetadata(null));

        public ICommand HitTestCommand
        {
            get => (ICommand)GetValue(HitTestCommandProperty);
            set => SetValue(HitTestCommandProperty, value);
        }

        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.MouseMove3D += OnMouseMove3D;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            AssociatedObject.MouseMove3D -= OnMouseMove3D;
        }

        private void OnMouseMove3D(object sender, RoutedEventArgs e)
        {
            if (e is not MouseMove3DEventArgs args) return;

            var hits = AssociatedObject.FindHits(args.Position);
            var hit = hits?.FirstOrDefault();

            if (HitTestCommand?.CanExecute(hit) == true)
            {
                HitTestCommand.Execute(hit);
            }
        }
    }
}
