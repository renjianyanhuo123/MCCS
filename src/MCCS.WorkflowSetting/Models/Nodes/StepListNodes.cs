using MCCS.Infrastructure;
using MCCS.WorkflowSetting.Components.ViewModels;
using MCCS.WorkflowSetting.Models.Edges;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using MCCS.WorkflowSetting.EventParams;

namespace MCCS.WorkflowSetting.Models.Nodes
{
    public class StepListNodes : BaseNode
    { 
        public const double _addActionDistance = 35.0; 

        public ObservableCollection<WorkflowConnection> Connections { get; private set; } = [];

        public ObservableCollection<BaseNode> Nodes { get; private set; } = [];

        private readonly EventHandler<AddNodeEvent> _addNodeEventHandler;
        private readonly EventHandler<DeleteNodeEvent> _deleteNodeEventHandler;

        /// <summary>
        /// 待插入节点的位置后面
        /// </summary>
        public BaseNode? InsertBeforeNode { get; private set; }

        public StepListNodes()
        {
            LoadedCommand = new RelayCommand(ExecuteLoadedCommand, _ => true);
            NodeClickCommand = new RelayCommand(ExecuteNodeClickCommand, _ => true);
            _addNodeEventHandler = (sender, obj) =>
            {
                if (obj.Source == Id)
                {
                    ExecuteAddNode(obj.Node);
                } 
            };
            _deleteNodeEventHandler = (sender, @event) =>
            {
                if (@event.Source == Id)
                {
                    ExecuteDeleteNode(@event);
                } 
            };
            EventMediator.Instance.Subscribe(_deleteNodeEventHandler);
            EventMediator.Instance.Subscribe(_addNodeEventHandler);
        }  
        #region Command 
        public ICommand LoadedCommand { get; }

        public ICommand NodeClickCommand { get; } 
        #endregion

        #region Private Method 
        private void ExecuteAddNode(BaseNode node)
        {
            var addOpNode = new AddOpNode
            {
                Name = "Add",
                Parent = this,
                Width = 20,
                Height = 20
            };  
            node.Parent = this;
            if (InsertBeforeNode == null)
            {
                Nodes.Add(node);
                Nodes.Add(addOpNode);
            }
            else
            {
                Nodes.Insert(InsertBeforeNode.Index, node);
                Nodes.Insert(InsertBeforeNode.Index + 1, addOpNode);
            }
            RenderChanged();
        }

        private void ExecuteDeleteNode(DeleteNodeEvent deleteEvent)
        {
            var deleteNodeInfo = Nodes.FirstOrDefault(c => c.Id == deleteEvent.NodeId); 
            if (deleteNodeInfo != null)
            {
                var deleteAddNode = Nodes.FirstOrDefault(c => c.Index == deleteNodeInfo.Index + 1);
                Nodes.Remove(deleteNodeInfo);
                if(deleteAddNode != null) Nodes.Remove(deleteAddNode);
                RenderChanged();
            }
        }

        private void ExecuteLoadedCommand(object? param)
        { 
            Nodes.Clear();
            Connections.Clear();
            Nodes.Add(new StartNode
            {
                Width = 60,
                Height = 60,
                Parent = this,
                Type = NodeTypeEnum.Start,
                Name = "Start"
            });
            Nodes.Add(new AddOpNode
            {
                Name = "Add",
                Parent = this,
                Type = NodeTypeEnum.Action,
                Width = 20,
                Height = 20
            });
            Nodes.Add(new EndNode
            {
                Name = "End",
                Parent = this,
                Type = NodeTypeEnum.End,
                Width = 56,
                Height = 80
            });
            RenderChanged();
        }

        private void ExecuteNodeClickCommand(object? param)
        {
            if (param is BaseNode node)
            {
                if (node.Type == NodeTypeEnum.Action)
                {
                    InsertBeforeNode = node;
                    EventMediator.Instance.Publish(new AddOpEventArgs
                    {
                        Source = Id,
                        NodeId = node.Id
                    });
                }
            }
        }

        public void UpdateNodePosition()
        {
            var maxLength = Nodes.Max(c => c.Width);
            Width = maxLength; 
            var count = Nodes.Count;
            for (var i = 0; i < count; i++)
            {
                Nodes[i].Index = i + 1;
                var yDistance = i == 0 ? 0 : Nodes[i - 1].Position.Y + Nodes[i - 1].Height + _addActionDistance; 
                Nodes[i].Position = new Point((maxLength - Nodes[i].Width) / 2.0, yDistance);
            } 
            Height = Nodes[count - 1].Position.Y + Nodes[count - 1].Height;
        }

        public void UpdateConnection()
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
        /// <summary>
        /// 渲染更新
        /// </summary>
        private void RenderChanged()
        {
            UpdateNodePosition();
            UpdateConnection();
        }

        protected override void ProcessNodeChange(NodeChangedEventArgs e)
        {
#if DEBUG
            Debug.WriteLine($"===主列表节点更新:{Id}===");
#endif
            RenderChanged();
            // 根节点处理完成后，可以标记事件为已处理，停止进一步传播
            e.Handled = true;
        }

        #endregion
    }
}
