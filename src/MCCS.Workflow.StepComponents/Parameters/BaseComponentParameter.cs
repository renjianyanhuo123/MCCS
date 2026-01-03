namespace MCCS.Workflow.StepComponents.Parameters
{
    /// <summary>
    /// 组件参数基类
    /// </summary>
    public abstract class BaseComponentParameter : IComponentParameter
    {
        public string Name { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public abstract ParameterType ParameterType { get; }
        public abstract Type ValueType { get; }
        public bool IsRequired { get; set; }
        public object? DefaultValue { get; set; }
        public object? Value { get; set; }
        public string Group { get; set; } = "常规";
        public int Order { get; set; }
        public bool IsVisible { get; set; } = true;
        public bool IsEditable { get; set; } = true;
        public string? Placeholder { get; set; }

        /// <summary>
        /// 最小值（用于数值类型）
        /// </summary>
        public object? MinValue { get; set; }

        /// <summary>
        /// 最大值（用于数值类型）
        /// </summary>
        public object? MaxValue { get; set; }

        /// <summary>
        /// 正则表达式验证（用于字符串类型）
        /// </summary>
        public string? ValidationPattern { get; set; }

        /// <summary>
        /// 验证失败消息
        /// </summary>
        public string? ValidationMessage { get; set; }

        public virtual ParameterValidationResult Validate()
        {
            // 必填验证
            if (IsRequired && (Value == null || (Value is string str && string.IsNullOrWhiteSpace(str))))
            {
                return ParameterValidationResult.Invalid($"{DisplayName} 是必填项");
            }

            return ParameterValidationResult.Valid();
        }

        public abstract IComponentParameter Clone();

        protected T CloneBase<T>(T target) where T : BaseComponentParameter
        {
            target.Name = Name;
            target.DisplayName = DisplayName;
            target.Description = Description;
            target.IsRequired = IsRequired;
            target.DefaultValue = DefaultValue;
            target.Value = Value;
            target.Group = Group;
            target.Order = Order;
            target.IsVisible = IsVisible;
            target.IsEditable = IsEditable;
            target.Placeholder = Placeholder;
            target.MinValue = MinValue;
            target.MaxValue = MaxValue;
            target.ValidationPattern = ValidationPattern;
            target.ValidationMessage = ValidationMessage;
            return target;
        }
    }
}
