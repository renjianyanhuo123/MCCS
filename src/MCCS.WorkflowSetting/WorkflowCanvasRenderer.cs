using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using MCCS.WorkflowSetting.Models.Edges;
using MCCS.WorkflowSetting.Models.Nodes;

namespace MCCS.WorkflowSetting
{
    /// <summary>
    /// 工作流Canvas渲染器
    /// </summary>
    public class WorkflowCanvasRenderer
    {
        private Canvas _canvas;
        private WorkflowGraph _workflowGraph;
        private BaseNode? _currentInsertBeforeNode = null;


        public WorkflowCanvasRenderer(Canvas canvas)
        {
            _canvas = canvas;
        }

        /// <summary>
        /// 渲染整个工作流
        /// </summary>
        public void RenderWorkflow(WorkflowGraph graph)
        {
            this._workflowGraph = graph;
            _canvas.Children.Clear();

            // 先绘制所有连接线（在节点下层）
            foreach (var connection in graph.AllConnections)
            {
                var converter = new BrushConverter();
                var brush1 = (Brush)converter.ConvertFromString("#CCCCCC")!;
                if (connection.Type == ConnectionTypeEnum.Sequential)
                {
                    _canvas.Children.Add(new Line
                    {
                        X1 = connection.SourcePoint.X,
                        Y1 = connection.SourcePoint.Y,
                        X2 = connection.TargetPoint.X,
                        Y2 = connection.TargetPoint.Y,
                        Stroke = brush1,
                        StrokeThickness = 1
                    });
                }
                else
                {
                    var polyline = new Polyline
                    {
                        Stroke = brush1,
                        StrokeThickness = 1
                    };
                    if (Math.Abs(connection.TargetPoint.Y - connection.SourcePoint.Y) > _workflowGraph.ChangedLimitValue)
                    {
                        var temp = connection.TargetPoint.Y - _workflowGraph.ChangedLimitValue / 2.0;
                        polyline.Points.Add(connection.SourcePoint);
                        polyline.Points.Add(
                            connection.SourcePoint 
                                with
                                {
                                    Y = temp
                            });
                        polyline.Points.Add(connection.TargetPoint 
                            with
                            {
                                Y = temp
                        });
                        polyline.Points.Add(connection.TargetPoint);
                    }
                    else
                    {
                        polyline.Points.Add(connection.SourceNode.CenterPoint);
                        polyline.Points.Add(new Point(
                            connection.TargetNode.CenterPoint.X, 
                            connection.SourceNode.CenterPoint.Y));
                        polyline.Points.Add(connection.TargetPoint);
                    }
                    
                    _canvas.Children.Add(polyline);
                }
            }

            // 再绘制所有节点（在连接线上层）
            foreach (var node in graph.AllNodes)
            {
                Canvas.SetLeft(node.Content, node.Position.X);
                Canvas.SetTop(node.Content, node.Position.Y);
                _canvas.Children.Add(node.Content);
            }
        }

        public void AddProcessNodeRender()
        {

        } 
    }
}
