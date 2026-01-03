namespace MCCS.Workflow.StepComponents.Core
{
    /// <summary>
    /// 组件执行结果
    /// </summary>
    public class ComponentExecutionResult
    {
        /// <summary>
        /// 执行状态
        /// </summary>
        public ComponentExecutionStatus Status { get; set; }

        /// <summary>
        /// 输出数据
        /// </summary>
        public IDictionary<string, object?> OutputData { get; set; } = new Dictionary<string, object?>();

        /// <summary>
        /// 错误消息
        /// </summary>
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// 异常信息
        /// </summary>
        public Exception? Exception { get; set; }

        /// <summary>
        /// 执行耗时（毫秒）
        /// </summary>
        public long ExecutionTimeMs { get; set; }

        /// <summary>
        /// 附加信息
        /// </summary>
        public IDictionary<string, object?> Metadata { get; set; } = new Dictionary<string, object?>();

        /// <summary>
        /// 创建成功结果
        /// </summary>
        public static ComponentExecutionResult Success(IDictionary<string, object?>? output = null)
        {
            return new ComponentExecutionResult
            {
                Status = ComponentExecutionStatus.Success,
                OutputData = output ?? new Dictionary<string, object?>()
            };
        }

        /// <summary>
        /// 创建失败结果
        /// </summary>
        public static ComponentExecutionResult Failure(string errorMessage, Exception? exception = null)
        {
            return new ComponentExecutionResult
            {
                Status = ComponentExecutionStatus.Failed,
                ErrorMessage = errorMessage,
                Exception = exception
            };
        }

        /// <summary>
        /// 创建取消结果
        /// </summary>
        public static ComponentExecutionResult Cancelled()
        {
            return new ComponentExecutionResult
            {
                Status = ComponentExecutionStatus.Cancelled
            };
        }

        /// <summary>
        /// 创建跳过结果
        /// </summary>
        public static ComponentExecutionResult Skipped(string? reason = null)
        {
            return new ComponentExecutionResult
            {
                Status = ComponentExecutionStatus.Skipped,
                ErrorMessage = reason
            };
        }
    }
}
