
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace MCCS.ViewModels.Pages
{
    public class HomeTestOperationPageViewModel : BaseViewModel
    {
        public const string Tag = "HomeTestOperationPage";

        private bool _action = false;
        private Point _startPoint;
        private Canvas _drawingCanvas;
        private Canvas _widgetCanvas;

        public HomeTestOperationPageViewModel(IEventAggregator eventAggregator, IDialogService dialogService) 
            : base(eventAggregator, dialogService)
        {
        }

        #region 命令
        public DelegateCommand<object> MouseRightButtonDownCommand => new(ExcuateMouseRightButtonDownCommand);
        public DelegateCommand<object> MouseMoveCommand => new(ExcuateMouseMoveCommand);
        public DelegateCommand<object> MouseRightButtonUpCommand => new(ExcuateMouseRightButtonUpCommand);
        public DelegateCommand<MouseWheelEventArgs> MouseWheelCommand => new(ExcuateMouseWheelCommand);
        #endregion

        #region 私有方法
        private void ExcuateMouseRightButtonDownCommand(object param) 
        {
            _drawingCanvas = param as Canvas;
            _startPoint = Mouse.GetPosition(_drawingCanvas);
            _action = true;
        }

        private void ExcuateMouseMoveCommand(object param) 
        {
            if (_action && Mouse.RightButton == MouseButtonState.Pressed) 
            {
                var widgetCanvas = param as Canvas;
                _widgetCanvas = widgetCanvas;
                var currentPoint = Mouse.GetPosition(_drawingCanvas);
                var d = currentPoint - _startPoint;
                var matrixTransform = widgetCanvas?.RenderTransform as MatrixTransform ?? throw new NullReferenceException("no widgetcanvas matrixtransform!");
                var matrix = matrixTransform.Matrix;
                matrix.OffsetX += d.X;
                matrix.OffsetY += d.Y;
                matrixTransform.Matrix = matrix;
                // 关键步骤, WidgetsCanvas关于 DrawingCanvas 真实的相对坐标
                //var transformedPoint = widgetCanvas?.TransformToAncestor(_drawingCanvas).Transform(new Point(0, 0));
                //widgetCanvas.Left = transformedPoint.X;
                //_commonSetting.WidgetsCanvas.Top = transformedPoint.Y;

                _startPoint = currentPoint;
            }
        }

        private void ExcuateMouseRightButtonUpCommand(object param) 
        {
            _action = false;
        }

        private void ExcuateMouseWheelCommand(MouseWheelEventArgs e) 
        {
            // 监听Crtl + 滚轮事件
            if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl)) 
            {
                var mousePosition = Mouse.GetPosition(_drawingCanvas);
                double scaleFactor = e.Delta > 0 ? 1.1 : 0.9;
                var widgetCanvasMatrix = _widgetCanvas.RenderTransform as MatrixTransform ?? throw new InvalidCastException("no MatrixTransform");
                //var oldTransformedPoint = _widgetCanvas.TransformToAncestor(_drawingCanvas).Transform(new Point(0, 0));
                var t = widgetCanvasMatrix.Matrix;
                t.ScaleAt(scaleFactor, scaleFactor, mousePosition.X, mousePosition.Y);
                widgetCanvasMatrix.Matrix = t;
                var transformedPoint = _widgetCanvas.TransformToAncestor(_drawingCanvas).Transform(new Point(0, 0));
                //_commonSetting.WidgetsCanvas.Left = transformedPoint.X;
                //_commonSetting.WidgetsCanvas.Top = transformedPoint.Y;
                //_widgetCanvas.Width *= scaleFactor;
                //_widgetCanvas.Height *= scaleFactor;
                _widgetCanvas.RenderTransform = widgetCanvasMatrix;
            }
        }
        #endregion
    }
}
