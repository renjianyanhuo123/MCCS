namespace MCCS.Workflow.StepComponents.Parameters
{
    /// <summary>
    /// 选择项
    /// </summary>
    public class SelectOption
    {
        /// <summary>
        /// 选项值
        /// </summary>
        public object? Value { get; set; }

        /// <summary>
        /// 显示文本
        /// </summary>
        public string DisplayText { get; set; } = string.Empty;

        /// <summary>
        /// 描述
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// 图标
        /// </summary>
        public string? Icon { get; set; }

        /// <summary>
        /// 是否禁用
        /// </summary>
        public bool IsDisabled { get; set; }

        public SelectOption() { }

        public SelectOption(object? value, string displayText)
        {
            Value = value;
            DisplayText = displayText;
        }
    }

    /// <summary>
    /// 下拉选择参数
    /// </summary>
    public class SelectParameter : BaseComponentParameter
    {
        public override ParameterType ParameterType => ParameterType.Select;
        public override Type ValueType => typeof(object);

        /// <summary>
        /// 选项列表
        /// </summary>
        public List<SelectOption> Options { get; set; } = new();

        /// <summary>
        /// 是否允许搜索
        /// </summary>
        public bool AllowSearch { get; set; }

        public override ParameterValidationResult Validate()
        {
            var baseResult = base.Validate();
            if (!baseResult.IsValid) return baseResult;

            if (Value != null && !Options.Any(o => Equals(o.Value, Value)))
            {
                return ParameterValidationResult.Invalid($"{DisplayName} 选择的值无效");
            }

            return ParameterValidationResult.Valid();
        }

        public override IComponentParameter Clone()
        {
            var clone = new SelectParameter();
            CloneBase(clone);
            clone.Options = Options.Select(o => new SelectOption
            {
                Value = o.Value,
                DisplayText = o.DisplayText,
                Description = o.Description,
                Icon = o.Icon,
                IsDisabled = o.IsDisabled
            }).ToList();
            clone.AllowSearch = AllowSearch;
            return clone;
        }
    }

    /// <summary>
    /// 多选参数
    /// </summary>
    public class MultiSelectParameter : BaseComponentParameter
    {
        public override ParameterType ParameterType => ParameterType.MultiSelect;
        public override Type ValueType => typeof(List<object>);

        /// <summary>
        /// 选项列表
        /// </summary>
        public List<SelectOption> Options { get; set; } = new();

        /// <summary>
        /// 最少选择数量
        /// </summary>
        public int? MinSelectCount { get; set; }

        /// <summary>
        /// 最多选择数量
        /// </summary>
        public int? MaxSelectCount { get; set; }

        public override ParameterValidationResult Validate()
        {
            var baseResult = base.Validate();
            if (!baseResult.IsValid) return baseResult;

            if (Value is IList<object> selectedValues)
            {
                if (MinSelectCount.HasValue && selectedValues.Count < MinSelectCount.Value)
                {
                    return ParameterValidationResult.Invalid($"{DisplayName} 至少需要选择 {MinSelectCount.Value} 项");
                }

                if (MaxSelectCount.HasValue && selectedValues.Count > MaxSelectCount.Value)
                {
                    return ParameterValidationResult.Invalid($"{DisplayName} 最多只能选择 {MaxSelectCount.Value} 项");
                }
            }

            return ParameterValidationResult.Valid();
        }

        public override IComponentParameter Clone()
        {
            var clone = new MultiSelectParameter();
            CloneBase(clone);
            clone.Options = Options.Select(o => new SelectOption
            {
                Value = o.Value,
                DisplayText = o.DisplayText,
                Description = o.Description,
                Icon = o.Icon,
                IsDisabled = o.IsDisabled
            }).ToList();
            clone.MinSelectCount = MinSelectCount;
            clone.MaxSelectCount = MaxSelectCount;
            return clone;
        }
    }
}
