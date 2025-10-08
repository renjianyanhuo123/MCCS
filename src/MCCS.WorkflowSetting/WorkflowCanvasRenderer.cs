using System.Windows.Controls;
using MCCS.WorkflowSetting.Models.Edges;
using MCCS.WorkflowSetting.Models.Nodes;

namespace MCCS.WorkflowSetting
{
    /// <summary>
    /// 工作流Canvas渲染器
    /// </summary>
    public class WorkflowCanvasRenderer : IWorkflowCanvasRenderer
    {
        private Canvas _canvas;
        private WorkflowGraph _workflowGraph;

        /// <summary>
        /// 第一步初始化
        /// </summary>
        /// <param name="canvas"></param>
        public void Initialize(Canvas canvas)
        {
            _canvas = canvas;
        }

        /// <summary>
        /// 渲染整个工作流
        /// </summary>
        public void RenderWorkflow(WorkflowGraph graph)
        {
            _workflowGraph = graph;
            _canvas.Children.Clear(); 
            // 先绘制所有连接线（在节点下层）
            foreach (var connection in graph.AllConnections)
            {
                if (connection.Type == ConnectionTypeEnum.Sequential)
                {
                    connection.IsRender = true;
                    _canvas.Children.Add(connection.LineElement);
                }
                else
                {
                    //var polyline = new Polyline
                    //{
                    //    Stroke = brush1,
                    //    StrokeThickness = 1
                    //};
                    //if (Math.Abs(connection.TargetPoint.Y - connection.SourcePoint.Y) > _workflowGraph.ChangedLimitValue)
                    //{
                    //    var temp = connection.TargetPoint.Y - _workflowGraph.ChangedLimitValue / 2.0;
                    //    polyline.Points.Add(connection.SourcePoint);
                    //    polyline.Points.Add(
                    //        connection.SourcePoint 
                    //            with
                    //            {
                    //                Y = temp
                    //            });
                    //    polyline.Points.Add(connection.TargetPoint 
                    //        with
                    //        {
                    //            Y = temp
                    //        });
                    //    polyline.Points.Add(connection.TargetPoint);
                    //}
                    //else
                    //{
                    //    polyline.Points.Add(connection.SourceNode.CenterPoint);
                    //    polyline.Points.Add(new Point(
                    //        connection.TargetNode.CenterPoint.X, 
                    //        connection.SourceNode.CenterPoint.Y));
                    //    polyline.Points.Add(connection.TargetPoint);
                    //}
                    
                    //_canvas.Children.Add(polyline);
                }
            }

            // 再绘制所有节点（在连接线上层）
            foreach (var node in graph.AllNodes)
            {
                node.IsRender = true;
                Canvas.SetLeft(node.Content, node.Position.X);
                Canvas.SetTop(node.Content, node.Position.Y);
                _canvas.Children.Add(node.Content);
            }
        }

        public void AddProcessNodeRender(BaseNode node)
        {
            if (_workflowGraph?.InsertBeforeNode == null) return; 
            _workflowGraph.AddNode(node);
            foreach (var connection in _workflowGraph.AllConnections.Where(connection => connection.IsRender == false))
            {
                connection.IsRender = true; 
                _canvas.Children.Add(connection.LineElement); 
            }
            foreach (var item in _workflowGraph.AllNodes)
            {
                if (item.IsRender == false)
                {
                    item.IsRender = true;
                    _canvas.Children.Add(item.Content);
                } 
                Canvas.SetLeft(item.Content, item.Position.X);
                Canvas.SetTop(item.Content, item.Position.Y);
            }
        } 
    }
}
