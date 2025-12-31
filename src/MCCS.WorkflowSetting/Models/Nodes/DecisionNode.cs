using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows;
using MCCS.WorkflowSetting.EventParams;

namespace MCCS.WorkflowSetting.Models.Nodes
{
    public class DecisionNode : BaseNode
    {

        #region Property
        /// <summary>
        /// 所有的子节点
        /// </summary>
        public ObservableCollection<BaseNode> Children { get; } = [];

        private bool _isCollapse = false;
        public bool IsCollapse
        {
            get => _isCollapse;
            set => SetProperty(ref _isCollapse, value);
        }

        private double _tempHeight;
        private double _tempWidth;

        private double _borderWidth; 
        public double BorderWidth
        {
            get => _borderWidth;
            set => SetProperty(ref _borderWidth, value);
        }

        private double _borderHeight;
        public double BorderHeight
        {
            get => _borderHeight;
            set => SetProperty(ref _borderHeight, value);
        }

        private Thickness _itemMargin = new(0, 0, 10, 0);
        public Thickness ItemMargin
        {
            get => _itemMargin;
            set => SetProperty(ref _itemMargin, value);
        }

        private Thickness _borderLeftMargin = new(0, 0, 10, 0);
        public Thickness BorderLeftMargin
        {
            get => _borderLeftMargin;
            set => SetProperty(ref _borderLeftMargin, value);
        }

        private double _borderLeftSpacing = 10;
        public double BorderLeftSpacing
        {
            get => _borderLeftSpacing;
            set
            {
                SetProperty(ref _borderLeftSpacing, value);
                BorderLeftMargin = new Thickness(value, 0, 0, 0);
            }
        }

        private double _itemSpacing = 10;
        public double ItemSpacing
        {
            get => _itemSpacing;
            set
            {
                SetProperty(ref _itemSpacing, value);
                var temp = value / 2.0;
                ItemMargin = new Thickness(temp, 0, temp, 0);
            }
        }

        private bool _isShowOperationBtn = false; 
        public bool IsShowOperationBtn
        {
            get => _isShowOperationBtn;
            set => SetProperty(ref _isShowOperationBtn, value);
        }

        #endregion

        public DecisionNode(IEventAggregator eventAggregator)
        {
            ItemSpacing = 10;
            var node1 = new BranchStepListNodes(eventAggregator)
            {
                Parent = this
            };
            var node2 = new BranchStepListNodes(eventAggregator)
            {
                Parent = this
            };
            IsCollapse = true;
            Width = ItemSpacing * 2 + node1.Width + node2.Width;
            Height = Math.Max(node1.Height, node2.Height);
            // 保存初始的展开状态尺寸
            _tempWidth = Width;
            _tempHeight = Height;
            Children.Add(node1);
            Children.Add(node2);
            ExecuteChildrenChanged();
            MouseLeaveCommand = new DelegateCommand(ExecuteMouseLeaveCommand);
            MouseEnterCommand = new DelegateCommand(ExecuteMouseEnterCommand);
            CollapseCommand = new DelegateCommand(ExecuteCollapseCommand);
        }

        private void ExecuteChildrenChanged()
        {
            var maxHeight = Children.Max(c => c.Height);
            var newWidth = ItemSpacing * Children.Count + Children.Sum(c => c.Width);
            var newHeight = maxHeight;

            BorderHeight = maxHeight;
            if (Children.Count > 1)
            {
                var rightSpace = ItemSpacing / 2.0 + Children[^1].Width / 2.0;
                var leftSpace = ItemSpacing / 2.0 + Children[0].Width / 2.0;
                BorderLeftSpacing = leftSpace;
                BorderWidth = newWidth - leftSpace - rightSpace;
            }

            // 只在展开状态下更新实际的Width和Height
            if (IsCollapse)
            {
                Width = newWidth;
                Height = newHeight;
                _tempWidth = newWidth;
                _tempHeight = newHeight;
            }
            else
            {
                // 收起状态下只更新临时尺寸，不改变实际显示尺寸
                _tempWidth = newWidth;
                _tempHeight = newHeight;
            }
        }

        #region Command
        public DelegateCommand MouseLeaveCommand { get; }
        public DelegateCommand MouseEnterCommand { get; }
        public DelegateCommand CollapseCommand { get; } 
        #endregion

        #region Private Method 
        private void ExecuteMouseEnterCommand() => IsShowOperationBtn = true;

        private void ExecuteMouseLeaveCommand() => IsShowOperationBtn = false;

        private void ExecuteCollapseCommand()
        {
            IsCollapse = !IsCollapse;
            if (!IsCollapse)  // 收起状态：隐藏分支，只显示中心图标
            {
                _tempHeight = Height;
                _tempWidth = Width;
                Height = 50;  // 只显示中心决策图标的高度
                Width = 100;  // 只显示中心决策图标的宽度
            }
            else  // 展开状态：恢复原来的尺寸
            {
                Height = _tempHeight;
                Width = _tempWidth;
            }

            // 通知父节点重新布局
            RaiseNodeChanged("Collapse");
        }

        #endregion

        #region 更新
        protected override void ProcessNodeChange(NodeChangedEventArgs e)
        {
            // 所有子流程节点更新以及整体更新
#if DEBUG
            Debug.WriteLine($"===决策节点更新:{Id}===");
#endif
            ExecuteChildrenChanged();
            // 先找出最大高度
            var maxHeight = 0.0;
            foreach (var child in Children)
            {
                if (child is BranchStepListNodes node)
                {
                    if (maxHeight < node.GetCurrentHeight)
                    {
                        maxHeight = node.GetCurrentHeight;
                    }
                }
            }

            BorderHeight = maxHeight;

            // 只在展开状态下更新Height
            if (IsCollapse)
            {
                Height = maxHeight;
                _tempHeight = maxHeight;
            }
            else
            {
                // 收起状态下只更新临时高度
                _tempHeight = maxHeight;
            }

            foreach (var child in Children)
            {
                if (child is BranchStepListNodes node)
                {
                    node.Height = maxHeight;
                    node.RenderChanged();
                }
            }
        }
        #endregion
    }
}
