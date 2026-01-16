using Microsoft.Xaml.Behaviors;
using System.Windows;
using System.Windows.Input;

namespace MCCS.Interface.Components.Behaviors
{
    /// <summary>
    /// 拖动缩放行为 - 用于通过拖动实现元素的缩放
    /// </summary>
    public class ResizeBehavior : Behavior<FrameworkElement>
    {
        #region Dependency Properties

        /// <summary>
        /// 目标宽度属性（双向绑定到 ViewModel）
        /// </summary>
        public static readonly DependencyProperty TargetWidthProperty =
            DependencyProperty.Register(
                nameof(TargetWidth),
                typeof(double),
                typeof(ResizeBehavior),
                new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        /// <summary>
        /// 目标高度属性（双向绑定到 ViewModel）
        /// </summary>
        public static readonly DependencyProperty TargetHeightProperty =
            DependencyProperty.Register(
                nameof(TargetHeight),
                typeof(double),
                typeof(ResizeBehavior),
                new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        /// <summary>
        /// 最小宽度
        /// </summary>
        public static readonly DependencyProperty MinWidthProperty =
            DependencyProperty.Register(
                nameof(MinWidth),
                typeof(double),
                typeof(ResizeBehavior),
                new PropertyMetadata(100.0));

        /// <summary>
        /// 最小高度
        /// </summary>
        public static readonly DependencyProperty MinHeightProperty =
            DependencyProperty.Register(
                nameof(MinHeight),
                typeof(double),
                typeof(ResizeBehavior),
                new PropertyMetadata(100.0));

        /// <summary>
        /// 最大宽度
        /// </summary>
        public static readonly DependencyProperty MaxWidthProperty =
            DependencyProperty.Register(
                nameof(MaxWidth),
                typeof(double),
                typeof(ResizeBehavior),
                new PropertyMetadata(double.MaxValue));

        /// <summary>
        /// 最大高度
        /// </summary>
        public static readonly DependencyProperty MaxHeightProperty =
            DependencyProperty.Register(
                nameof(MaxHeight),
                typeof(double),
                typeof(ResizeBehavior),
                new PropertyMetadata(double.MaxValue));

        #endregion

        #region Properties

        public double TargetWidth
        {
            get => (double)GetValue(TargetWidthProperty);
            set => SetValue(TargetWidthProperty, value);
        }

        public double TargetHeight
        {
            get => (double)GetValue(TargetHeightProperty);
            set => SetValue(TargetHeightProperty, value);
        }

        public double MinWidth
        {
            get => (double)GetValue(MinWidthProperty);
            set => SetValue(MinWidthProperty, value);
        }

        public double MinHeight
        {
            get => (double)GetValue(MinHeightProperty);
            set => SetValue(MinHeightProperty, value);
        }

        public double MaxWidth
        {
            get => (double)GetValue(MaxWidthProperty);
            set => SetValue(MaxWidthProperty, value);
        }

        public double MaxHeight
        {
            get => (double)GetValue(MaxHeightProperty);
            set => SetValue(MaxHeightProperty, value);
        }

        #endregion

        #region Private Fields

        private bool _isDragging;
        private Point _startPoint;
        private double _startWidth;
        private double _startHeight;

        #endregion

        #region Behavior Overrides

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

        #endregion

        #region Event Handlers

        private void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (AssociatedObject.CaptureMouse())
            {
                _isDragging = true;
                _startPoint = e.GetPosition(null);
                _startWidth = TargetWidth;
                _startHeight = TargetHeight;
                e.Handled = true;
            }
        }

        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            if (!_isDragging) return;

            var currentPoint = e.GetPosition(null);
            var deltaX = currentPoint.X - _startPoint.X;
            var deltaY = currentPoint.Y - _startPoint.Y;

            // 计算新的宽度和高度
            var newWidth = Math.Clamp(_startWidth + deltaX, MinWidth, MaxWidth);
            var newHeight = Math.Clamp(_startHeight + deltaY, MinHeight, MaxHeight);

            TargetWidth = newWidth;
            TargetHeight = newHeight;
        }

        private void OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (_isDragging)
            {
                _isDragging = false;
                AssociatedObject.ReleaseMouseCapture();
                e.Handled = true;
            }
        }

        #endregion
    }
}
