namespace MCCS.WorkflowSetting.Models.Nodes
{
    public class BranchNode : BaseNode
    {
        private readonly IEventAggregator _eventAggregator;

        public BranchNode(IEventAggregator eventAggregator, BaseNode? parent)
        {
            _eventAggregator = eventAggregator;
            Name = "Branch";
            Parent = parent;
            Title = "分支";
            Width = 260;
            Height = 85;
            Type = NodeTypeEnum.Branch;
            DeleteSingleDecisionCommand = new DelegateCommand(ExecuteDeleteSingleDecisionCommand);
            OperationNodeClickedCommand = new DelegateCommand(ExecuteOperationNodeClickedCommand);
            DeleteNodeCommand = new DelegateCommand(ExecuteDeleteNodeCommand);
            ConfigueDeleteCommand = new DelegateCommand(ExecuteConfigueDeleteCommand);
            CancelCommand = new DelegateCommand(ExecuteCancelCommand); 
        }

        #region Property
        /// <summary>
        /// 节点标题
        /// </summary>
        private string _title = string.Empty;
        public string Title
        {
            get => _title;
            set => SetProperty(ref _title, value);
        }

        private bool _isOpen = false;
        public bool IsOpen
        {
            get => _isOpen;
            set => SetProperty(ref _isOpen, value);
        }

        private bool _isShowShade = false;
        public bool IsShowShade
        {
            get => _isShowShade;
            set => SetProperty(ref _isShowShade, value);
        }
        #endregion

        #region Command 
        public DelegateCommand DeleteSingleDecisionCommand { get; }
        public DelegateCommand OperationNodeClickedCommand { get; }
        public DelegateCommand DeleteNodeCommand { get; }
        public DelegateCommand CancelCommand { get; }
        public DelegateCommand ConfigueDeleteCommand { get; }

        #endregion

        #region Private Method
        private void ExecuteDeleteSingleDecisionCommand()
        {
            IsShowShade = true;
            IsOpen = false;
        }

        private void ExecuteOperationNodeClickedCommand()
        {
            IsOpen = true;
        }

        private void ExecuteDeleteNodeCommand()
        {
            IsShowShade = true;
            IsOpen = false;
        }

        private void ExecuteCancelCommand()
        {
            IsShowShade = false;
        }

        private void ExecuteConfigueDeleteCommand()
        {
            // BranchNode的Parent是BranchStepListNodes
            if (Parent is not BranchStepListNodes branchStepList) return;
            // BranchStepListNodes的Parent是DecisionNode
            if (branchStepList.Parent is not DecisionNode decisionNode) return;
            // 从DecisionNode的Children集合中删除整个BranchStepListNodes
            // 这会删除当前子分支下的所有内容
            decisionNode.Children.Remove(branchStepList);
            // 更新决策分支数量
            decisionNode.DecisionNum = decisionNode.Children.Count;
            // 触发变更事件，通知UI更新
            decisionNode.RaiseNodeChanged("DeleteBranch");
            // 隐藏遮罩层
            IsShowShade = false;
        }

        #endregion
    }
}
