using MCCS.WorkflowSetting.Models.Edges;
using System.Collections.ObjectModel;
using MCCS.WorkflowSetting.EventParams;

namespace MCCS.WorkflowSetting.Models.Nodes
{
    public class BoxListNodes : BaseNode
    {
        public double AddActionDistance { get; protected set; } = 35.0;

        protected readonly IEventAggregator _eventAggregator;

        public ObservableCollection<WorkflowConnection> Connections { get; } = [];

        public ObservableCollection<BaseNode> Nodes { get; } = [];

        /// <summary>
        /// 待插入节点的位置后面 
        /// </summary>
        protected BaseNode? InsertBeforeNode { get; private set; } 
        public BoxListNodes(IEventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator;
            LoadedCommand = new DelegateCommand(() => { });
            NodeClickCommand = new DelegateCommand<object?>(ExecuteNodeClickCommand);
            _eventAggregator.GetEvent<AddNodeEvent>().Subscribe(ExecuteAddNode);
            _eventAggregator.GetEvent<DeleteNodeEvent>().Subscribe(ExecuteDeleteNode);
            _eventAggregator.GetEvent<DeleteTempPlaceholderNodeEvent>()
                .Subscribe(OnDeleteTempPlaceholderNodeEvent, ThreadOption.UIThread, false, filter => filter.SourceId != Id);
        }

        #region Command 
        public DelegateCommand LoadedCommand { get; protected set; }

        public DelegateCommand<object?> NodeClickCommand { get; protected set; }
        #endregion

        #region Private Method 
        private void OnDeleteTempPlaceholderNodeEvent(DeleteTempPlaceholderNodeEventParam param)
        {
            // 删除当前List节点中所有的占位节点
            var deleteNodeInfo = Nodes.FirstOrDefault(c => c.Type == NodeTypeEnum.TempPlaceholder);
            if (deleteNodeInfo == null) return;
            Nodes.Remove(deleteNodeInfo);
            RaiseNodeChanged("UIChanged", "");
        }

        protected void ExecuteAddNode(AddNodeEventParam param)
        {
            if (param == null) return;
            if (param.Source != Id) return;
            var deleteNodeInfo = Nodes.FirstOrDefault(c => c.Type == NodeTypeEnum.TempPlaceholder);
            if (deleteNodeInfo == null) return;
            Nodes.Remove(deleteNodeInfo);
            var node = param.Node;
            var addOpNode = new AddOpNode(this);
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
            
            // 触发更新; 因为会先更新自己的节点变更
            RaiseNodeChanged("UIChanged", "");
        }
        /// <summary>
        /// 节点点击
        /// </summary>
        /// <param name="param"></param>
        protected void ExecuteNodeClickCommand(object? param)
        {
            if (param is not BaseNode { Type: NodeTypeEnum.Action } node) return;
            InsertBeforeNode = node;
            // 先删除之前在界面中非母线(即自己所在的List)所有的剩余占位点
            _eventAggregator.GetEvent<DeleteTempPlaceholderNodeEvent>().Publish(new DeleteTempPlaceholderNodeEventParam
            {
                PublisherId = node.Id,
                SourceId = Id
            });
            var deleteNodeInfo = Nodes.FirstOrDefault(c => c.Type == NodeTypeEnum.TempPlaceholder);
            if (deleteNodeInfo != null) Nodes.Remove(deleteNodeInfo);
            Nodes.Insert(InsertBeforeNode.Index, new TempPlaceholderAddNode(this));
            RaiseNodeChanged("UIChanged", "");
            _eventAggregator.GetEvent<AddOpEvent>().Publish(new AddOpEventParam
            {
                Source = Id,
                NodeId = node.Id
            });
        }
        /// <summary>
        /// 删除节点
        /// </summary>
        /// <param name="param"></param>
        protected void ExecuteDeleteNode(DeleteNodeEventParam param)
        {
            if (param == null || param.Source != Id) return;
            var deleteNodeInfo = Nodes.FirstOrDefault(c => c.Id == param.NodeId);
            if (deleteNodeInfo == null) return;
            var deleteAddNode = Nodes.FirstOrDefault(c => c.Index == deleteNodeInfo.Index + 1);
            Nodes.Remove(deleteNodeInfo);
            if (deleteAddNode != null) Nodes.Remove(deleteAddNode);
            // 触发更新
            RaiseNodeChanged("UIChanged", "");
        }
        #endregion

        /// <summary>
        /// 渲染更新
        /// </summary>
        public void RenderChanged()
        {
            UpdateNodePosition();
            UpdateConnection();
        }
        /// <summary>
        /// (2)更新连接线
        /// </summary>
        protected virtual void UpdateConnection()
        {  }
        /// <summary>
        /// (1)更新节点位置
        /// </summary>
        protected virtual void UpdateNodePosition()
        { }
    }
}
