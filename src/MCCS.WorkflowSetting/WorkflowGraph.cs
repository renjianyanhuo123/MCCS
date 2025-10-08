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

        public const double _addActionDistance = 35.0;
        private const double _startNodeHeight = 80.0;
        private Action _openFlyout; 

        /// <summary>
        /// 待插入节点的位置后面
        /// </summary>
        public BaseNode? InsertBeforeNode { get; private set; }

        public double ChangedLimitValue => _addActionDistance * 2.0;

        public string Name { get; set; } 
        public List<BaseNode> AllNodes { get; private set; }
        public List<WorkflowConnection> AllConnections { get; private set; }
        /// <summary>
        /// 添加按钮前置的操作
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="param"></param>
        private void AddOpBeforeHandle(AddOpEventArgs param)
        {
            if (param == null) return;
            InsertBeforeNode = AllNodes.FirstOrDefault(c => c.Id == param.NodeId);
            _openFlyout?.Invoke();
        }

        /// <summary>
        /// 构建一个全新的工作流图/未渲染
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="addOp">添加外部需要执行的逻辑</param>
        public WorkflowGraph(double width, double height, Action openFlyout)
        {
            _canvasWidth = width;
            _canvasHeight = height;
            _openFlyout = openFlyout;
            AllNodes = [];
            AllConnections = [];
            var t1 = new BaseNode("Start", NodeTypeEnum.Start, 60, 60);
            var t2 = new AddOpNode("Add", NodeTypeEnum.Action, 20, 20, AddOpBeforeHandle);
            var t3 = new BaseNode("End", NodeTypeEnum.End, 56, 80);
            AllNodes.Add(t1);
            AllNodes.Add(t2);
            AllNodes.Add(t3);
            UpdateNodePosition();
            UpdateConnection();
        }

        public void UpdateNodePosition()
        {
            for (var i = 0; i < AllNodes.Count; i++)
            {
                AllNodes[i].Index = i + 1;
                var yDistance = i == 0 ? _startNodeHeight : AllNodes[i - 1].Position.Y + AllNodes[i - 1].Height + _addActionDistance;
                AllNodes[i].Position = new Point(_canvasWidth / 2.0 - AllNodes[i].Width / 2.0, yDistance);
            }
        }

        public void UpdateConnection()
        {
            for (var i = 1; i < AllNodes.Count; i++)
            {
                var startPoint = AllNodes[i - 1].CenterPoint with { Y = AllNodes[i - 1].Position.Y + AllNodes[i - 1].Height };
                var endPoint = new Point(AllNodes[i].CenterPoint.X, AllNodes[i].Position.Y);
                if (i <= AllConnections.Count)
                {
                    AllConnections[i - 1].SourcePoint = startPoint;
                    AllConnections[i - 1].TargetPoint = endPoint;
                }
                else
                {
                    AllConnections.Add(new WorkflowConnection()
                    {
                        Id = Guid.NewGuid().ToString("N"),
                        SourcePoint = startPoint,
                        TargetPoint = endPoint,
                        Type = startPoint.X == endPoint.X ? ConnectionTypeEnum.Sequential : ConnectionTypeEnum.Conditional
                    });
                }
            }
        }

        public void AddNode(BaseNode node)
        { 
            if (AllNodes.Contains(node)) return;
            var inseredList = new List<BaseNode>
            {
                node,
                new AddOpNode("Add", NodeTypeEnum.Action, 20, 20, AddOpBeforeHandle)
            };
            if (InsertBeforeNode == null)
            {
                AllNodes.AddRange(inseredList); 
            }
            else
            {
                AllNodes.InsertRange(InsertBeforeNode.Index, inseredList);
            }
            UpdateNodePosition();
            UpdateConnection();
        } 
        //#region Private Method  
        //#endregion
    }
}
