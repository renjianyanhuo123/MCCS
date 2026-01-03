namespace MCCS.Workflow.StepComponents.Serialization
{
    /// <summary>
    /// 组件实例数据传输对象（用于序列化）
    /// </summary>
    public class ComponentInstanceDto
    {
        /// <summary>
        /// 实例ID（唯一标识此实例）
        /// </summary>
        public string InstanceId { get; set; } = Guid.NewGuid().ToString("N");

        /// <summary>
        /// 组件类型ID
        /// </summary>
        public string ComponentId { get; set; } = string.Empty;

        /// <summary>
        /// 组件名称（用于显示）
        /// </summary>
        public string DisplayName { get; set; } = string.Empty;

        /// <summary>
        /// 参数值字典
        /// </summary>
        public Dictionary<string, object?> ParameterValues { get; set; } = new();

        /// <summary>
        /// 是否启用
        /// </summary>
        public bool IsEnabled { get; set; } = true;

        /// <summary>
        /// 备注
        /// </summary>
        public string? Remarks { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        /// <summary>
        /// 最后修改时间
        /// </summary>
        public DateTime ModifiedAt { get; set; } = DateTime.Now;
    }
}
