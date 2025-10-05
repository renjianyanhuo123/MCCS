using System.Windows;
using MCCS.WorkflowSetting.EventParams;
using MCCS.WorkflowSetting.Models.Edges;
using MCCS.WorkflowSetting.Models.Nodes;

namespace MCCS.WorkflowSetting
{
    public class WorkflowGraph
    {
        private double _canvasWidth = -1;
        private double _canvasHeight = -1;

        public const double _addActionDistance = 16.0;
        private const double _startNodeHeight = 80.0;

        /// <summary>
        /// 待插入节点的位置后面
        /// </summary>
        public BaseNode? InsertBeforeNode { get; private set; }

        public double ChangedLimitValue => _addActionDistance * 2.0;

        public string Name { get; set; }
        public BaseNode StartNode { get; private set; }
        public BaseNode EndNode { get; private set; }
        public List<BaseNode> AllNodes { get; private set; }
        public List<WorkflowConnection> AllConnections { get; private set; }
        /// <summary>
        /// 添加按钮前置的操作
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="param"></param>
        private void AddOpBeforeHandle(object? sender, AddOpEventArgs param)
        {
            if (param == null) return;
            InsertBeforeNode = AllNodes.FirstOrDefault(c => c.Id == param.NodeId);
        }

        /// <summary>
        /// 构建一个全新的工作流图/未渲染
        /// </summary>
        /// <param name="width">画布宽</param>
        /// <param name="height">画布高</param>
        public WorkflowGraph(double width, double height)
        {
            _canvasWidth = width;
            _canvasHeight = height;
            AllNodes = [];
            AllConnections = [];
            StartNode = new BaseNode("Start", NodeTypeEnum.Start,80, 80);
            var addOpBtn = new AddOpNode("add", NodeTypeEnum.Action, 20, 20, AddOpBeforeHandle);
            EndNode = new BaseNode("End", NodeTypeEnum.End, 112, 80);
            AddNode(StartNode);
            AddNode(addOpBtn, StartNode);
            AddNode(EndNode, addOpBtn);
            AddConnection(StartNode, addOpBtn);
            AddConnection(addOpBtn, EndNode);
        }

        /// <summary>
        /// 添加节点
        /// </summary>
        /// <param name="node">待添加节点</param>
        /// <param name="insertAfterNode">添加到该节点后面</param>
        /// <exception cref="ArgumentNullException"></exception>
        public void AddNode(BaseNode node, BaseNode? insertAfterNode = null)
        {
            if (AllNodes.Contains(node)) return;
            // 计算布局
            if (node.Type == NodeTypeEnum.Start)
            {
                // 开始节点必在中轴线上
                node.Position = new Point(_canvasWidth / 2.0 - node.Width / 2.0, _startNodeHeight);
                node.Level = 0;
                StartNode = node; 
            }
            else if (node.Type is NodeTypeEnum.Action or NodeTypeEnum.End)
            {
                if (insertAfterNode == null) throw new ArgumentNullException(nameof(insertAfterNode));
                insertAfterNode.Children =
                [
                    node
                ];
                node.Position = new Point(
                    insertAfterNode.CenterPoint.X - node.Width / 2.0,
                    insertAfterNode.Position.Y + insertAfterNode.Height + _addActionDistance);
                node.Level = insertAfterNode.Level + 1;
                node.Order = 1;
            }
            else
            {
                // 其他节点，暂时不处理位置

            }
            AllNodes.Add(node);
        }

        /// <summary>
        /// 添加连接线
        /// </summary>
        /// <param name="startBaseNode"></param>
        /// <param name="endNode"></param>
        public void AddConnection(BaseNode startBaseNode, BaseNode endNode)
        {
            var startPoint = startBaseNode.CenterPoint with { Y = startBaseNode.Position.Y + startBaseNode.Height };
            var endPoint = new Point(endNode.CenterPoint.X, endNode.Position.Y);
            if (AllConnections.Any(c => c.SourcePoint == startPoint && c.TargetPoint == endPoint)) return;
            var connection = new WorkflowConnection()
            {
                Id = Guid.NewGuid().ToString("N"),
                SourceNode = startBaseNode,
                TargetNode = endNode,
                SourcePoint = startPoint,
                TargetPoint = endPoint,
                Type = startPoint.X == endPoint.X ? ConnectionTypeEnum.Sequential : ConnectionTypeEnum.Conditional
            };
            AllConnections.Add(connection);
        }
    }
}
