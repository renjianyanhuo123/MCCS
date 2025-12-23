namespace MCCS.Components.LayoutRootComponents.ViewModels
{
    public abstract class LayoutNode : BindableBase
    {
        /// <summary>
        /// 单元格唯一标识符
        /// </summary>
        private string _id = Guid.NewGuid().ToString("N"); 
        public string Id 
        { 
            get => _id;
            set => SetProperty(ref _id, value);
        } 
        ///// <summary>
        ///// 父节点
        ///// </summary>
        private LayoutNode? _parent;
        public LayoutNode? Parent { get => _parent; set => SetProperty(ref _parent, value); }

        /// <summary>
        /// 左子节点
        /// </summary>
        private LayoutNode? _leftNode;
        public LayoutNode? LeftNode { get => _leftNode;
            set
            { 
                SetProperty(ref _leftNode, value);
            }
        }

        /// <summary>
        /// 右子节点
        /// </summary>
        private LayoutNode? _rightNode;
        public LayoutNode? RightNode { get => _rightNode;
            set
            { 
                SetProperty(ref _rightNode, value);
            }
        }
    }
}
