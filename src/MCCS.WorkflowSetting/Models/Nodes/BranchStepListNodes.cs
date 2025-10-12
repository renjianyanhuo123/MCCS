using MCCS.WorkflowSetting.EventParams;
using MCCS.WorkflowSetting.Models.Edges;
using System.Diagnostics;
using System.Windows;

namespace MCCS.WorkflowSetting.Models.Nodes
{
    public class BranchStepListNodes : BoxListNodes
    {
        public BranchStepListNodes(IEventAggregator eventAggregator) : base(eventAggregator)
        {
            Type = NodeTypeEnum.BranchStepList;
            Width = 260;
            Height = 200; 
            LoadedCommand = new DelegateCommand(ExecuteLoadedCommand); 
        } 
        #region Private Method
        private void ExecuteLoadedCommand()
        {
            Nodes.Clear();
            Connections.Clear();
            Nodes.Add(new BranchNode
            {
                Name = "Branch",
                Parent = this,
                Width = 260,
                Height = 85
            });
            Nodes.Add(new AddOpNode
            {
                Name = "Add",
                Parent = this,
                Width = 20,
                Height = 20
            });
            RenderChanged();
        }
        
        protected override void UpdateNodePosition()
        {
            var maxLength = Nodes.Max(c => c.Width);
            Width = maxLength;
            var count = Nodes.Count;
            for (var i = 0; i < count; i++)
            {
                Nodes[i].Index = i + 1;
                var yDistance = i == 0 ? AddActionDistance : Nodes[i - 1].Position.Y + Nodes[i - 1].Height + AddActionDistance;
                Nodes[i].Position = new Point((maxLength - Nodes[i].Width) / 2.0, yDistance);
            }
            var tempHeight = Nodes[count - 1].Position.Y + Nodes[count - 1].Height + AddActionDistance;  
            if (tempHeight > Height)
            {
                Height = Nodes[count - 1].Position.Y + Nodes[count - 1].Height + AddActionDistance;
            }
            //if (Parent is DecisionNode parentNode)
            //{
            //    Height = parentNode.Children.Max(c => c.Height); 
            //}
        }

        public double GetCurrentHeight => Nodes[^1].Position.Y + Nodes[^1].Height + AddActionDistance;

        protected override void UpdateConnection()
        {
            var xDistance = Nodes[0].CenterPoint.X;
            for (var i = 0; i <= Nodes.Count; i++)
            {
                var startPointYDistance = 0.0;
                var endPointYDistance = 0.0;
                if (i != 0 && i < Nodes.Count)
                {
                    startPointYDistance = Nodes[i - 1].Position.Y + Nodes[i - 1].Height;
                    endPointYDistance = Nodes[i].Position.Y;
                }
                else if (i >= Nodes.Count)
                {
                    startPointYDistance = Nodes[i - 1].Position.Y + Nodes[i - 1].Height;
                    endPointYDistance = Height;
                }
                else if (i == 0)
                {
                    startPointYDistance = 0.0;
                    endPointYDistance = Nodes[i].Position.Y;
                }
                var startPoint = new Point(xDistance, startPointYDistance);
                var endPoint = new Point(xDistance, endPointYDistance);
                if (i < Connections.Count)
                {
                    Connections[i].Points = [startPoint, endPoint];
                }
                else
                {
                    Connections.Add(new WorkflowConnection
                    {
                        Id = Guid.NewGuid().ToString("N"),
                        Type = startPoint.X == endPoint.X ? ConnectionTypeEnum.Sequential : ConnectionTypeEnum.Conditional,
                        Points = [startPoint, endPoint]
                    });
                }
            }
            // 去除掉后面多余的线
            for (var i = Connections.Count - 1; i >= Nodes.Count + 1; i--)
            {
                Connections.RemoveAt(i);
            }
        }
        #endregion

        #region 节点更新 
        protected override void ProcessNodeChange(NodeChangedEventArgs e)
        {
#if DEBUG
            Debug.WriteLine($"===分支子流程更新:{Id}===");
#endif
            RenderChanged();
        } 
        #endregion
    }
}
