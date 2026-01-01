using System.Collections.Generic;
using MCCS.WorkflowSetting.Models.Edges;

namespace MCCS.WorkflowSetting.Serialization.Dtos
{
    /// <summary>
    /// 连接线序列化数据传输对象
    /// </summary>
    public class ConnectionDto
    {
        /// <summary>
        /// 连接线ID
        /// </summary>
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// 连接类型
        /// </summary>
        public ConnectionTypeEnum Type { get; set; }

        /// <summary>
        /// 起始节点ID
        /// </summary>
        public string SourceNodeId { get; set; } = string.Empty;

        /// <summary>
        /// 目标节点ID
        /// </summary>
        public string TargetNodeId { get; set; } = string.Empty;

        /// <summary>
        /// 路径点集合
        /// </summary>
        public List<PointDto> Points { get; set; } = new();

        /// <summary>
        /// 扩展属性
        /// </summary>
        public Dictionary<string, object> Properties { get; set; } = new();
    }
}
