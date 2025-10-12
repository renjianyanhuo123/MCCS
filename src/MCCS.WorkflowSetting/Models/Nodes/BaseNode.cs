using System.Windows; 
using MCCS.WorkflowSetting.EventParams;

namespace MCCS.WorkflowSetting.Models.Nodes
{
    public abstract class BaseNode : BindableBase
    {  
        public string Id { get; private set; } = Guid.NewGuid().ToString("N");
        public int Index { get; set; } 

        private BaseNode? _parent;
        /// <summary>
        /// 父节点
        /// </summary>
        public BaseNode? Parent
        {
            get => _parent;
            set => SetProperty(ref _parent, value);
        }

        /// <summary>
        /// 节点变更事件
        /// </summary>
        public event EventHandler<NodeChangedEventArgs> NodeChanged;
        /// <summary>
        /// 用于快速查找父级节点
        /// 000001-000002,000004-000008-000009
        /// 最后一个表示数字表示自身的ID
        /// 前面的表示其分支节点号
        /// 默认伪6位数
        /// 000001
        /// </summary>
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;

        private double _width = 0; 
        public double Width
        {
            get => _width; 
            set => SetProperty(ref _width, value);
        }

        private double _height = 0;
        public double Height { 
            get => _height; 
            set => SetProperty(ref _height, value);
        }
        /// <summary>
        /// 根节点默认为-1，其他节点表示其自身在同一父级中的顺序;从1开始
        /// </summary>
        public int Order { get; set; }

        /// <summary>
        /// 节点处于的层级
        /// </summary>
        public int Level { get; set; }

        private Point _position;
        public Point Position
        {
            get => _position;
            set
            {
                SetProperty(ref _position, value);
                CenterPoint = new Point(_position.X + Width / 2, _position.Y + Height / 2);
            }
        } 
        /// <summary>
        /// 中心点坐标
        /// </summary>
        public Point CenterPoint { get; private set; }
        public NodeTypeEnum Type { get; set; }

        /// <summary>
        /// 触发变更事件（从当前节点开始向上冒泡）
        /// </summary>
        protected void RaiseNodeChanged(string changeType, object changeData = null)
        {
            var args = new NodeChangedEventArgs(this, changeType, changeData);
            OnNodeChanged(args);
        }

        /// <summary>
        /// 处理节点变更事件
        /// </summary>
        protected virtual void OnNodeChanged(NodeChangedEventArgs e)
        {
            // 先执行当前节点的处理逻辑
            ProcessNodeChange(e);

            // 触发事件给外部订阅者
            NodeChanged?.Invoke(this, e);

            // 如果事件未被标记为已处理，继续向上传播
            if (!e.Handled && Parent != null)
            {
                Parent.OnNodeChanged(e);
            }
        }

        /// <summary>
        /// 处理节点变更的核心逻辑（子类可重写）
        /// </summary>
        protected virtual void ProcessNodeChange(NodeChangedEventArgs e)
        {
            // 基类默认不做处理
        }

        /// <summary>
        /// 父节点改变时的回调
        /// </summary>
        protected virtual void OnParentChanged()
        {
        }

        /// <summary>
        /// 更新节点内部状态（可被子类重写）
        /// </summary>
        public virtual void UpdateInternalState()
        {
            // 基类默认实现
        }
    }
}
