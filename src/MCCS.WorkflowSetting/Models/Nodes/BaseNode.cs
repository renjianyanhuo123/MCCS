using System.Windows;
using MCCS.WorkflowSetting.Components.ViewModels;

namespace MCCS.WorkflowSetting.Models.Nodes
{
    public class BaseNode : BindingBase
    {  
        public string Id { get; private set; } = Guid.NewGuid().ToString("N");
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
    }
}
