using MCCS.Workflow.StepComponents.Core;

namespace MCCS.Workflow.StepComponents.Registry
{
    /// <summary>
    /// 组件信息（用于展示和选择）
    /// </summary>
    public class ComponentInfo
    {
        /// <summary>
        /// 组件唯一标识
        /// </summary>
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// 组件名称
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 组件描述
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// 组件分类
        /// </summary>
        public ComponentCategory Category { get; set; }

        /// <summary>
        /// 组件图标
        /// </summary>
        public string Icon { get; set; } = "Cog";

        /// <summary>
        /// 组件版本
        /// </summary>
        public string Version { get; set; } = "1.0.0";

        /// <summary>
        /// 作者
        /// </summary>
        public string Author { get; set; } = string.Empty;

        /// <summary>
        /// 标签
        /// </summary>
        public string[] Tags { get; set; } = Array.Empty<string>();

        /// <summary>
        /// 是否启用
        /// </summary>
        public bool IsEnabled { get; set; } = true;

        /// <summary>
        /// 排序顺序
        /// </summary>
        public int Order { get; set; }

        /// <summary>
        /// 组件类型
        /// </summary>
        public Type ComponentType { get; set; } = null!;
    }
}
