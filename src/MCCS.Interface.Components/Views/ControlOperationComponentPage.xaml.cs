using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

using MCCS.Interface.Components.ViewModels.ControlOperationComponents;

namespace MCCS.Interface.Components.Views
{
    /// <summary>
    /// ControlOperationComponentPage.xaml 的交互逻辑
    /// </summary>
    public partial class ControlOperationComponentPage
    {
        private bool _isResizing;
        private Point _startPoint;
        private double _startWidth;
        private double _startHeight;
        private ControlUnitComponent? _resizingUnit;

        public ControlOperationComponentPage()
        {
            InitializeComponent();
        }

        private void ResizeHandle_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is not Border border) return;

            // 获取DataContext（ControlUnitComponent）
            var parent = border.Parent as Grid;
            var outerBorder = parent?.Parent as Border;
            if (outerBorder?.DataContext is not ControlUnitComponent unit) return;

            _isResizing = true;
            _startPoint = e.GetPosition(this);
            _startWidth = unit.Width;
            _startHeight = unit.Height;
            _resizingUnit = unit;

            border.CaptureMouse();
            e.Handled = true;
        }

        private void ResizeHandle_MouseMove(object sender, MouseEventArgs e)
        {
            if (!_isResizing || _resizingUnit == null) return;

            var currentPoint = e.GetPosition(this);
            var deltaX = currentPoint.X - _startPoint.X;
            var deltaY = currentPoint.Y - _startPoint.Y;

            // 计算新的宽度和高度，确保不小于最小值
            const double minWidth = 150;
            const double minHeight = 200;

            var newWidth = Math.Max(minWidth, _startWidth + deltaX);
            var newHeight = Math.Max(minHeight, _startHeight + deltaY);

            _resizingUnit.Width = newWidth;
            _resizingUnit.Height = newHeight;

            e.Handled = true;
        }

        private void ResizeHandle_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (!_isResizing) return;

            _isResizing = false;
            _resizingUnit = null;

            if (sender is Border border)
            {
                border.ReleaseMouseCapture();
            }

            e.Handled = true;
        }
    }
}
