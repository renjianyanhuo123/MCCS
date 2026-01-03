using MCCS.Workflow.StepComponents.Parameters;

namespace MCCS.Workflow.StepComponents.Attributes
{
    /// <summary>
    /// 参数属性，用于标记组件的参数属性
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class ParameterAttribute : Attribute
    {
        /// <summary>
        /// 参数名称（用于代码引用）
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// 显示名称
        /// </summary>
        public string DisplayName { get; set; } = string.Empty;

        /// <summary>
        /// 参数描述
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// 参数类型
        /// </summary>
        public ParameterType ParameterType { get; set; } = ParameterType.String;

        /// <summary>
        /// 是否必填
        /// </summary>
        public bool IsRequired { get; set; } = false;

        /// <summary>
        /// 参数分组
        /// </summary>
        public string Group { get; set; } = "常规";

        /// <summary>
        /// 排序顺序
        /// </summary>
        public int Order { get; set; } = 0;

        /// <summary>
        /// 提示文本
        /// </summary>
        public string? Placeholder { get; set; }

        public ParameterAttribute(string name)
        {
            Name = name;
        }
    }
}
