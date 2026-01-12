using MCCS.Workflow.Contact.Models;
using MCCS.Workflow.StepComponents.Enums;

namespace MCCS.Workflow.StepComponents.Attributes
{
    /// <summary>
    /// 步骤组件属性，用于标记和描述组件
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class StepComponentAttribute(string id, string name, NodeDisplayTypeEnum displayType = NodeDisplayTypeEnum.Step) : Attribute
    {
        /// <summary>
        /// 组件唯一标识
        /// </summary>
        public string Id { get; } = id;

        /// <summary>
        /// 组件名称
        /// </summary>
        public string Name { get; } = name;

        /// <summary>
        /// 展示类型
        /// </summary>
        public NodeDisplayTypeEnum DisplayType { get; } = displayType;

        /// <summary>
        /// 组件描述
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// 组件分类
        /// </summary>
        public ComponentCategory Category { get; set; } = ComponentCategory.General;

        /// <summary>
        /// 组件图标（Material Design Icon名称）
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
        /// 是否启用
        /// </summary>
        public bool IsEnabled { get; set; } = true;

        /// <summary>
        /// 排序顺序
        /// </summary>
        public int Order { get; set; } = 0;

        /// <summary>
        /// 标签（用于搜索和筛选）
        /// </summary>
        public string[] Tags { get; set; } = [];
    }
}
