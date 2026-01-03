namespace MCCS.Workflow.StepComponents.Parameters
{
    /// <summary>
    /// 组件参数接口
    /// </summary>
    public interface IComponentParameter
    {
        /// <summary>
        /// 参数名称（用于代码引用）
        /// </summary>
        string Name { get; }

        /// <summary>
        /// 显示名称
        /// </summary>
        string DisplayName { get; }

        /// <summary>
        /// 参数描述
        /// </summary>
        string Description { get; }

        /// <summary>
        /// 参数类型
        /// </summary>
        ParameterType ParameterType { get; }

        /// <summary>
        /// .NET类型
        /// </summary>
        Type ValueType { get; }

        /// <summary>
        /// 是否必填
        /// </summary>
        bool IsRequired { get; }

        /// <summary>
        /// 默认值
        /// </summary>
        object? DefaultValue { get; }

        /// <summary>
        /// 当前值
        /// </summary>
        object? Value { get; set; }

        /// <summary>
        /// 参数分组
        /// </summary>
        string Group { get; }

        /// <summary>
        /// 排序顺序
        /// </summary>
        int Order { get; }

        /// <summary>
        /// 是否可见
        /// </summary>
        bool IsVisible { get; set; }

        /// <summary>
        /// 是否可编辑
        /// </summary>
        bool IsEditable { get; set; }

        /// <summary>
        /// 提示文本
        /// </summary>
        string? Placeholder { get; }

        /// <summary>
        /// 验证参数值
        /// </summary>
        ParameterValidationResult Validate();

        /// <summary>
        /// 克隆参数
        /// </summary>
        IComponentParameter Clone();
    }
}
