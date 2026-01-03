namespace MCCS.Workflow.StepComponents.Core
{
    /// <summary>
    /// 组件验证结果
    /// </summary>
    public class ComponentValidationResult
    {
        /// <summary>
        /// 是否验证通过
        /// </summary>
        public bool IsValid { get; set; }

        /// <summary>
        /// 验证错误列表
        /// </summary>
        public List<ValidationError> Errors { get; set; } = new();

        /// <summary>
        /// 验证警告列表
        /// </summary>
        public List<ValidationWarning> Warnings { get; set; } = new();

        /// <summary>
        /// 创建成功的验证结果
        /// </summary>
        public static ComponentValidationResult Valid()
        {
            return new ComponentValidationResult { IsValid = true };
        }

        /// <summary>
        /// 创建失败的验证结果
        /// </summary>
        public static ComponentValidationResult Invalid(params ValidationError[] errors)
        {
            return new ComponentValidationResult
            {
                IsValid = false,
                Errors = errors.ToList()
            };
        }

        /// <summary>
        /// 添加错误
        /// </summary>
        public ComponentValidationResult AddError(string parameterName, string message)
        {
            Errors.Add(new ValidationError(parameterName, message));
            IsValid = false;
            return this;
        }

        /// <summary>
        /// 添加警告
        /// </summary>
        public ComponentValidationResult AddWarning(string parameterName, string message)
        {
            Warnings.Add(new ValidationWarning(parameterName, message));
            return this;
        }
    }

    /// <summary>
    /// 验证错误
    /// </summary>
    public record ValidationError(string ParameterName, string Message);

    /// <summary>
    /// 验证警告
    /// </summary>
    public record ValidationWarning(string ParameterName, string Message);
}
