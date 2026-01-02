using MCCS.WorkflowSetting.Models.Nodes;

namespace MCCS.WorkflowSetting.Serialization.Dtos
{
    /// <summary>
    /// 节点DTO基类，用于序列化
    /// </summary>
    public class BaseNodeDto
    {
        /// <summary>
        /// 节点唯一标识
        /// </summary>
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// 节点名称
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 节点编码（用于快速查找父级节点）
        /// </summary>
        public string Code { get; set; } = string.Empty;

        /// <summary>
        /// 节点类型
        /// </summary>
        public NodeTypeEnum Type { get; set; }

        /// <summary>
        /// 节点顺序
        /// </summary>
        public int Order { get; set; }

        /// <summary>
        /// 节点层级
        /// </summary>
        public int Level { get; set; }
    }
}
