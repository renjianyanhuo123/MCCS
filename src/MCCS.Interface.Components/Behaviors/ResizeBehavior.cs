using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

using Microsoft.Xaml.Behaviors;

namespace MCCS.Interface.Components.Behaviors
{
    /// <summary>
    /// 用于实现拖动缩放的行为
    /// 将此行为附加到拖动手柄元素上，通过修改目标元素的Width和Height实现缩放
    /// </summary>
    public class ResizeBehavior : Behavior<FrameworkElement>
    {
        private bool _isResizing;
        private Point _startPoint;
        private double _startWidth;
        private double _startHeight;

        #region Dependency Properties

        public static readonly DependencyProperty TargetElementProperty =
            DependencyProperty.Register(
                nameof(TargetElement),
                typeof(FrameworkElement),
                typeof(ResizeBehavior),
                new PropertyMetadata(null));

        public static readonly DependencyProperty MinWidthProperty =
            DependencyProperty.Register(
                nameof(MinWidth),
                typeof(double),
                typeof(ResizeBehavior),
                new PropertyMetadata(150.0));

        public static readonly DependencyProperty MinHeightProperty =
            DependencyProperty.Register(
                nameof(MinHeight),
                typeof(double),
                typeof(ResizeBehavior),
                new PropertyMetadata(200.0));

        /// <summary>
        /// 要调整大小的目标元素
        /// </summary>
        public FrameworkElement? TargetElement
        {
            get => (FrameworkElement?)GetValue(TargetElementProperty);
            set => SetValue(TargetElementProperty, value);
        }

        /// <summary>
        /// 最小宽度
        /// </summary>
        public double MinWidth
        {
            get => (double)GetValue(MinWidthProperty);
            set => SetValue(MinWidthProperty, value);
        }

        /// <summary>
        /// 最小高度
        /// </summary>
        public double MinHeight
        {
            get => (double)GetValue(MinHeightProperty);
            set => SetValue(MinHeightProperty, value);
        }

        #endregion

        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.MouseLeftButtonDown += OnMouseLeftButtonDown;
            AssociatedObject.MouseMove += OnMouseMove;
            AssociatedObject.MouseLeftButtonUp += OnMouseLeftButtonUp;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            AssociatedObject.MouseLeftButtonDown -= OnMouseLeftButtonDown;
            AssociatedObject.MouseMove -= OnMouseMove;
            AssociatedObject.MouseLeftButtonUp -= OnMouseLeftButtonUp;
        }

        private FrameworkElement? GetTargetElement()
        {
            if (TargetElement != null)
                return TargetElement;

            // 自动查找：向上遍历找到带有Width/Height绑定的Border
            var parent = AssociatedObject.Parent as FrameworkElement;
            while (parent != null)
            {
                if (parent is Border)
                    return parent;
                parent = parent.Parent as FrameworkElement;
            }

            return null;
        }

        private void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var target = GetTargetElement();
            if (target == null) return;

            _isResizing = true;
            _startPoint = e.GetPosition(Application.Current.MainWindow);
            _startWidth = target.Width;
            _startHeight = target.Height;

            AssociatedObject.CaptureMouse();
            e.Handled = true;
        }

        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            if (!_isResizing) return;

            var target = GetTargetElement();
            if (target == null) return;

            var currentPoint = e.GetPosition(Application.Current.MainWindow);
            var deltaX = currentPoint.X - _startPoint.X;
            var deltaY = currentPoint.Y - _startPoint.Y;

            var newWidth = Math.Max(MinWidth, _startWidth + deltaX);
            var newHeight = Math.Max(MinHeight, _startHeight + deltaY);

            target.Width = newWidth;
            target.Height = newHeight;

            e.Handled = true;
        }

        private void OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (!_isResizing) return;

            _isResizing = false;
            AssociatedObject.ReleaseMouseCapture();
            e.Handled = true;
        }
    }
}
