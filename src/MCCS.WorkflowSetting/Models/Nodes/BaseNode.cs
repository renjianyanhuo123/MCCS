using System.Windows;
using MCCS.WorkflowSetting.Components;

namespace MCCS.WorkflowSetting.Models.Nodes
{
    public class BaseNode
    {
        public BaseNode(
            string name, 
            NodeTypeEnum type, 
            double width, 
            double height, 
            int level = 0,
            int order = -1)
        {
            Id = Guid.NewGuid().ToString("N");
            Name = name;
            Type = type;
            Width = width;
            Height = height;
            Order = order;
            Level = level;
            VirtualWidth = width;
            VirtualHeight = height;
            Content = CreateContent();
            Content.Width = width;
            Content.Height = height;
        }

        public string Id { get; private set; }
        public string Name { get; private set; }
        public double Width { get; private set; }
        public double Height { get; private set; }
        /// <summary>
        /// 根节点默认为-1，其他节点表示其自身在同一父级中的顺序；从1开始
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
                if (_position != value)
                {
                    _position = value;
                    CenterPoint = new Point(_position.X + Width / 2, _position.Y + Height / 2);
                }

            }
        }

        /// <summary>
        /// 中心点坐标
        /// </summary>
        public Point CenterPoint { get; private set; }
        public NodeTypeEnum Type { get; set; }
        /// <summary>
        /// 所有的子节点
        /// </summary>
        public List<BaseNode> Children { get; set; }

        /// <summary>
        /// 虚拟宽度
        /// </summary>
        public double VirtualWidth { get; private set; }
        /// <summary>
        /// 虚拟高度
        /// </summary>
        public double VirtualHeight { get; private set; }

        public FrameworkElement Content { get; private set; }

        private FrameworkElement CreateContent()
        {
            switch (Type)
            {
                case NodeTypeEnum.Start:
                    return new WorkflowStartNode();
                case NodeTypeEnum.End:
                    return new WorkflowEndNode();
                case NodeTypeEnum.Process:
                    return new WorkflowStepNode();
                case NodeTypeEnum.Decision:
                    return new WorkflowDecisionNode();
                case NodeTypeEnum.Action:
                    return new WorkflowAddOperationNode();
                default:
                    return new WorkflowStepNode();
            }
        }
    }
}
