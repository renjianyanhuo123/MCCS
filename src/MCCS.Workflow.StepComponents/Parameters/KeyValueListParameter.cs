namespace MCCS.Workflow.StepComponents.Parameters
{
    /// <summary>
    /// 键值对项
    /// </summary>
    public class KeyValueItem
    {
        /// <summary>
        /// 键
        /// </summary>
        public string Key { get; set; } = string.Empty;

        /// <summary>
        /// 值
        /// </summary>
        public string Value { get; set; } = string.Empty;

        /// <summary>
        /// 是否启用
        /// </summary>
        public bool IsEnabled { get; set; } = true;
    }

    /// <summary>
    /// 键值对列表参数
    /// </summary>
    public class KeyValueListParameter : BaseComponentParameter
    {
        public override ParameterType ParameterType => ParameterType.KeyValueList;
        public override Type ValueType => typeof(List<KeyValueItem>);

        /// <summary>
        /// 键标签
        /// </summary>
        public string KeyLabel { get; set; } = "键";

        /// <summary>
        /// 值标签
        /// </summary>
        public string ValueLabel { get; set; } = "值";

        /// <summary>
        /// 是否允许添加
        /// </summary>
        public bool AllowAdd { get; set; } = true;

        /// <summary>
        /// 是否允许删除
        /// </summary>
        public bool AllowDelete { get; set; } = true;

        /// <summary>
        /// 是否允许重复键
        /// </summary>
        public bool AllowDuplicateKeys { get; set; } = false;

        public override ParameterValidationResult Validate()
        {
            var baseResult = base.Validate();
            if (!baseResult.IsValid) return baseResult;

            if (Value is List<KeyValueItem> items && !AllowDuplicateKeys)
            {
                var duplicates = items.Where(i => i.IsEnabled)
                    .GroupBy(i => i.Key)
                    .Where(g => g.Count() > 1)
                    .Select(g => g.Key)
                    .ToList();

                if (duplicates.Any())
                {
                    return ParameterValidationResult.Invalid($"{DisplayName} 包含重复的键: {string.Join(", ", duplicates)}");
                }
            }

            return ParameterValidationResult.Valid();
        }

        public override IComponentParameter Clone()
        {
            var clone = new KeyValueListParameter();
            CloneBase(clone);
            clone.KeyLabel = KeyLabel;
            clone.ValueLabel = ValueLabel;
            clone.AllowAdd = AllowAdd;
            clone.AllowDelete = AllowDelete;
            clone.AllowDuplicateKeys = AllowDuplicateKeys;
            return clone;
        }
    }
}
