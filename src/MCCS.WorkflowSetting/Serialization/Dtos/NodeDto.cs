using MCCS.WorkflowSetting.Models.Nodes;

namespace MCCS.WorkflowSetting.Serialization.Dtos
{
    /// <summary>
    /// 节点序列化数据传输对象
    /// </summary>
    public class NodeDto
    {
        /// <summary>
        /// 节点唯一标识
        /// </summary>
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// 节点类型
        /// </summary>
        public NodeTypeEnum Type { get; set; }

        /// <summary>
        /// 节点名称
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 节点编码（层级编码）
        /// </summary>
        public string Code { get; set; } = string.Empty;

        /// <summary>
        /// 索引
        /// </summary>
        public int Index { get; set; }

        /// <summary>
        /// 在同一父级中的顺序
        /// </summary>
        public int Order { get; set; }

        /// <summary>
        /// 所在层级
        /// </summary>
        public int Level { get; set; }

        /// <summary>
        /// 位置X坐标
        /// </summary>
        public double PositionX { get; set; }

        /// <summary>
        /// 位置Y坐标
        /// </summary>
        public double PositionY { get; set; }

        /// <summary>
        /// 宽度
        /// </summary>
        public double Width { get; set; }

        /// <summary>
        /// 高度
        /// </summary>
        public double Height { get; set; }

        /// <summary>
        /// 子节点列表（用于BoxListNodes）
        /// </summary>
        public List<NodeDto> Children { get; set; } = [];

        /// <summary>
        /// 特定类型节点的扩展属性
        /// </summary>
        public Dictionary<string, object> Properties { get; set; } = new();
    }
}
