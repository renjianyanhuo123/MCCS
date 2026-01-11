using System.Collections.ObjectModel;

using MCCS.WorkflowSetting.Models.Edges;
using System.Diagnostics;
using System.Windows;
using MCCS.WorkflowSetting.EventParams;

namespace MCCS.WorkflowSetting.Models.Nodes
{
    public class StepListNodes : BoxListNodes
    { 

        public StepListNodes(IEventAggregator eventAggregator, IDialogService dialogService, List<BaseNode> children) : base(eventAggregator, dialogService)
        {
            Type = NodeTypeEnum.StepList;
            children.ForEach(node => node.Parent = this);
            Nodes.AddRange(children);
            LoadedCommand = new DelegateCommand(ExecuteLoadedCommand); 
        }


        #region Private Method  
        private void ExecuteLoadedCommand()
        { 
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
                var yDistance = i == 0 ? 0 : Nodes[i - 1].Position.Y + Nodes[i - 1].Height + AddActionDistance;
                Nodes[i].Position = new Point((maxLength - Nodes[i].Width) / 2.0, yDistance);
            } 
            Height = Nodes[count - 1].Position.Y + Nodes[count - 1].Height + AddActionDistance;
        }

        protected override void UpdateConnection()
        {
            for (var i = 1; i < Nodes.Count; i++)
            {
                var startPoint = Nodes[i - 1].CenterPoint with { Y = Nodes[i - 1].Position.Y + Nodes[i - 1].Height };
                var endPoint = new Point(Nodes[i].CenterPoint.X, Nodes[i].Position.Y);
                if (i <= Connections.Count)
                {
                    Connections[i - 1].Points = [startPoint, endPoint];
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
            for (var i = Connections.Count - 1; i >= Nodes.Count - 1; i--)
            {
                Connections.RemoveAt(i);
            }
        } 
        #endregion

        #region 处理节点变更  
        protected override void ProcessNodeChange(NodeChangedEventArgs e)
        {
#if DEBUG
            Debug.WriteLine($"===主列表节点更新:{Id}===");
#endif
            RenderChanged();
            _eventAggregator.GetEvent<NotificationWorkflowChangedEvent>().Publish(new NotificationWorkflowChangedEventParam()
            {
                Width = Width,
                Height = Height
            });
            // 根节点处理完成后，可以标记事件为已处理，停止进一步传播
            e.Handled = true;
        } 
        #endregion
    }
}
