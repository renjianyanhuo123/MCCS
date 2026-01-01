using System.Collections.Generic;

namespace MCCS.WorkflowSetting.Serialization.Dtos
{
    /// <summary>
    /// 工作流序列化数据传输对象
    /// </summary>
    public class WorkflowDto
    {
        /// <summary>
        /// 工作流ID
        /// </summary>
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// 工作流名称
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 工作流版本
        /// </summary>
        public string Version { get; set; } = "1.0.0";

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// 最后修改时间
        /// </summary>
        public DateTime UpdatedAt { get; set; }

        /// <summary>
        /// 根节点（主流程）
        /// </summary>
        public NodeDto? RootNode { get; set; }

        /// <summary>
        /// 所有连接线
        /// </summary>
        public List<ConnectionDto> Connections { get; set; } = new();

        /// <summary>
        /// 扩展属性（用于存储其他元数据）
        /// </summary>
        public Dictionary<string, object> Metadata { get; set; } = new();
    }
}
