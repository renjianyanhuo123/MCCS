namespace MCCS.Workflow.StepComponents.Parameters
{
    /// <summary>
    /// 参数验证结果
    /// </summary>
    public class ParameterValidationResult
    {
        /// <summary>
        /// 是否有效
        /// </summary>
        public bool IsValid { get; set; }

        /// <summary>
        /// 错误消息
        /// </summary>
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// 创建有效结果
        /// </summary>
        public static ParameterValidationResult Valid() => new() { IsValid = true };

        /// <summary>
        /// 创建无效结果
        /// </summary>
        public static ParameterValidationResult Invalid(string message) => new() { IsValid = false, ErrorMessage = message };
    }
}
