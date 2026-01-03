namespace MCCS.Workflow.StepComponents.Parameters
{
    /// <summary>
    /// 文件路径参数
    /// </summary>
    public class FilePathParameter : BaseComponentParameter
    {
        public override ParameterType ParameterType => ParameterType.FilePath;
        public override Type ValueType => typeof(string);

        /// <summary>
        /// 文件过滤器（例如："文本文件|*.txt|所有文件|*.*"）
        /// </summary>
        public string? Filter { get; set; }

        /// <summary>
        /// 默认扩展名
        /// </summary>
        public string? DefaultExtension { get; set; }

        /// <summary>
        /// 是否必须存在
        /// </summary>
        public bool MustExist { get; set; } = true;

        /// <summary>
        /// 对话框标题
        /// </summary>
        public string? DialogTitle { get; set; }

        public override ParameterValidationResult Validate()
        {
            var baseResult = base.Validate();
            if (!baseResult.IsValid) return baseResult;

            if (Value is string path && !string.IsNullOrWhiteSpace(path))
            {
                if (MustExist && !File.Exists(path))
                {
                    return ParameterValidationResult.Invalid($"{DisplayName} 指定的文件不存在");
                }
            }

            return ParameterValidationResult.Valid();
        }

        public override IComponentParameter Clone()
        {
            var clone = new FilePathParameter();
            CloneBase(clone);
            clone.Filter = Filter;
            clone.DefaultExtension = DefaultExtension;
            clone.MustExist = MustExist;
            clone.DialogTitle = DialogTitle;
            return clone;
        }
    }

    /// <summary>
    /// 文件夹路径参数
    /// </summary>
    public class FolderPathParameter : BaseComponentParameter
    {
        public override ParameterType ParameterType => ParameterType.FolderPath;
        public override Type ValueType => typeof(string);

        /// <summary>
        /// 是否必须存在
        /// </summary>
        public bool MustExist { get; set; } = true;

        /// <summary>
        /// 对话框描述
        /// </summary>
        public string? DialogDescription { get; set; }

        public override ParameterValidationResult Validate()
        {
            var baseResult = base.Validate();
            if (!baseResult.IsValid) return baseResult;

            if (Value is string path && !string.IsNullOrWhiteSpace(path))
            {
                if (MustExist && !Directory.Exists(path))
                {
                    return ParameterValidationResult.Invalid($"{DisplayName} 指定的文件夹不存在");
                }
            }

            return ParameterValidationResult.Valid();
        }

        public override IComponentParameter Clone()
        {
            var clone = new FolderPathParameter();
            CloneBase(clone);
            clone.MustExist = MustExist;
            clone.DialogDescription = DialogDescription;
            return clone;
        }
    }
}
