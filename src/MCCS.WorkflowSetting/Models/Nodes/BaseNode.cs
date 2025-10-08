using System.Windows;
using MCCS.WorkflowSetting.Components;
using MCCS.WorkflowSetting.Components.ViewModels;

namespace MCCS.WorkflowSetting.Models.Nodes
{
    public class BaseNode : BindingBase
    {
        public BaseNode(
            string name, 
            NodeTypeEnum type, 
            double width, 
            double height, 
            int level = 0,
            int order = -1):this(name, width, height, level, order)
        {  
            Type = type;
            Content = CreateContent();
            Content.Width = width;
            Content.Height = height;
        }

        public BaseNode(
            string name, 
            double width,
            double height,
            int level = 0,
            int order = -1)
        {
            Id = Guid.NewGuid().ToString("N");
            Name = name; 
            Width = width;
            Height = height;
            Order = order;
            Level = level; 
        }

        public string Id { get; private set; }
        public int Index { get; set; }
        public bool IsRender { get; set; }

        /// <summary>
        /// 用于快速查找父级节点
        /// 000001-000002,000004-000008-000009
        /// 最后一个表示数字表示自身的ID
        /// 前面的表示其分支节点号
        /// 默认伪6位数
        /// 000001
        /// </summary>
        public string Code { get; set; } = string.Empty;
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
        public FrameworkElement Content { get; protected set; }

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
