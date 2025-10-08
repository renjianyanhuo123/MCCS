using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace MCCS.WorkflowSetting.Models.Edges
{
    public class WorkflowConnection
    {
        public WorkflowConnection()
        {
            LineElement = new Line
            {
                Stroke = (Brush)converter.ConvertFromString("#CCCCCC")!,
                StrokeThickness = 1
            };
            Panel.SetZIndex(LineElement, int.MinValue);
        }

        private static BrushConverter converter = new();

        public string Id { get; set; } 
        public ConnectionTypeEnum Type { get; set; }
        public string Label { get; set; } = string.Empty; // 分支条件标签

        /// <summary>
        /// 是否已渲染
        /// </summary>
        public bool IsRender { get; set; } = false;

        // 连接点位置（相对于节点的位置）
        private Point _sourcePoint;
        public Point SourcePoint
        {
            get => _sourcePoint;
            set
            {
                if (_sourcePoint != value)
                {
                    _sourcePoint = value;
                    LineElement.X1 = _sourcePoint.X;
                    LineElement.Y1 = _sourcePoint.Y;
                }
            }
        }

        private Point _targetPoint; 
        public Point TargetPoint
        {
            get => _targetPoint;
            set
            {
                if (_targetPoint != value)
                {
                    _targetPoint = value;
                    LineElement.X2 = _targetPoint.X;
                    LineElement.Y2 = _targetPoint.Y;
                }
            }
        }

        public Line LineElement { get; }
    }
}
