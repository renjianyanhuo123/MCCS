using MCCS.Infrastructure;
using MCCS.WorkflowSetting.Components.ViewModels;
using MCCS.WorkflowSetting.EventParams;
using MCCS.WorkflowSetting.Models.Edges;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;

namespace MCCS.WorkflowSetting.Models.Nodes
{
    public class BranchStepListNodes : BaseNode
    {
        public const double _addActionDistance = 35.0;

        public ObservableCollection<WorkflowConnection> Connections { get; private set; } = [];

        public ObservableCollection<BaseNode> Nodes { get; private set; } = [];

        private readonly EventHandler<AddNodeEvent> _addNodeEventHandler;
        private readonly EventHandler<DeleteNodeEvent> _deleteNodeEventHandler;
        private readonly EventHandler<NotificationBranchChangedEvent> _branchChangedEventHandler;

        /// <summary>
        /// 待插入节点的位置后面 
        /// </summary>
        public BaseNode? InsertBeforeNode { get; private set; }

        public BranchStepListNodes(EventHandler<NotificationBranchChangedEvent> branchChangedEventHandler)
        {
            Type = NodeTypeEnum.BranchStepList;
            Width = 260;
            Height = 200;
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
            _branchChangedEventHandler = branchChangedEventHandler;
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
                Type = NodeTypeEnum.Action,
                Width = 20,
                Height = 20
            };
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
            UpdateNodePosition();
            UpdateConnection();
        }

        private void ExecuteDeleteNode(DeleteNodeEvent deleteEvent)
        {
            var deleteNodeInfo = Nodes.FirstOrDefault(c => c.Id == deleteEvent.NodeId);
            if (deleteNodeInfo != null)
            {
                var deleteAddNode = Nodes.FirstOrDefault(c => c.Index == deleteNodeInfo.Index + 1);
                Nodes.Remove(deleteNodeInfo);
                if (deleteAddNode != null) Nodes.Remove(deleteAddNode);
                UpdateNodePosition();
                UpdateConnection();
            }
        }

        private void ExecuteLoadedCommand(object? param)
        {
            Nodes.Clear();
            Connections.Clear();
            Nodes.Add(new BranchNode
            {
                Name = "Branch",
                Width = 260,
                Height = 85
            });
            Nodes.Add(new AddOpNode
            {
                Name = "Add",
                Width = 20,
                Height = 20
            });
            UpdateNodePosition();
            UpdateConnection();
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
                var yDistance = i == 0 ? _addActionDistance : Nodes[i - 1].Position.Y + Nodes[i - 1].Height + _addActionDistance;
                Nodes[i].Position = new Point((maxLength - Nodes[i].Width) / 2.0, yDistance);
            }
            Height = Nodes[count - 1].Position.Y + Nodes[count - 1].Height;
            // 通知分支内其他节点发生了变化
            _branchChangedEventHandler.Invoke(null, new NotificationBranchChangedEvent
            {
                Source = ""
            });
        }

        public void UpdateConnection()
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
    }
}
