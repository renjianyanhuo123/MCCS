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
        #endregion

        public DecisionNode()
        {
            ItemSpacing = 10; 
            var node1 = new BranchStepListNodes
            {
                Parent = this
            };
            var node2 = new BranchStepListNodes
            {
                Parent = this
            };
            Width = ItemSpacing * 2 + node1.Width + node2.Width;
            Height = Math.Max(node1.Height, node2.Height); 
            Children.Add(node1);
            Children.Add(node2);
            ExecuteChildrenChanged();
        }

        private void ExecuteChildrenChanged()
        {
            var maxHeight = Children.Max(c => c.Height);
            Width = ItemSpacing * Children.Count + Children.Sum(c => c.Width);
            Height = maxHeight;
            foreach (var node in Children)
            {
                node.Height = maxHeight;
            }
            BorderHeight = maxHeight;
            if (Children.Count > 1)
            {
                var rightSpace = ItemSpacing / 2.0 + Children[^1].Width / 2.0;
                var leftSpace = ItemSpacing / 2.0 + Children[0].Width / 2.0;
                BorderLeftSpacing = leftSpace;
                BorderWidth = Width - leftSpace - rightSpace;
            }
        }

        #region 更新 
        protected override void ProcessNodeChange(NodeChangedEventArgs e)
        {
            // 所有子流程节点更新以及整体更新
#if DEBUG 
            Debug.WriteLine($"===决策节点更新:{Id}===");
#endif
            ExecuteChildrenChanged();
            // 重新更新所有的直系子流程
            foreach (var child in Children)
            {
                // 排除已经更新过的节点
                if (child == e.SourceNode) continue;
                if (child is BranchStepListNodes node)
                {
                    node.RenderChanged();
                }
            }
        } 
        #endregion
    }
}
