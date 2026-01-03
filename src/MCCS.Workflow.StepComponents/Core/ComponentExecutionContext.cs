namespace MCCS.Workflow.StepComponents.Core
{
    /// <summary>
    /// 组件执行上下文
    /// </summary>
    public class ComponentExecutionContext
    {
        /// <summary>
        /// 工作流ID
        /// </summary>
        public string WorkflowId { get; set; } = string.Empty;

        /// <summary>
        /// 工作流实例ID
        /// </summary>
        public string WorkflowInstanceId { get; set; } = string.Empty;

        /// <summary>
        /// 当前步骤ID
        /// </summary>
        public string StepId { get; set; } = string.Empty;

        /// <summary>
        /// 当前步骤名称
        /// </summary>
        public string StepName { get; set; } = string.Empty;

        /// <summary>
        /// 全局变量（跨步骤共享）
        /// </summary>
        public IDictionary<string, object?> GlobalVariables { get; set; } = new Dictionary<string, object?>();

        /// <summary>
        /// 本地变量（当前步骤）
        /// </summary>
        public IDictionary<string, object?> LocalVariables { get; set; } = new Dictionary<string, object?>();

        /// <summary>
        /// 上一步的输出
        /// </summary>
        public IDictionary<string, object?> PreviousStepOutput { get; set; } = new Dictionary<string, object?>();

        /// <summary>
        /// 日志记录委托
        /// </summary>
        public Action<string, LogLevel>? Log { get; set; }

        /// <summary>
        /// 进度报告委托
        /// </summary>
        public Action<int, string>? ReportProgress { get; set; }

        /// <summary>
        /// 获取变量值（优先查找本地变量，再查找全局变量）
        /// </summary>
        public T? GetVariable<T>(string name)
        {
            if (LocalVariables.TryGetValue(name, out var localValue) && localValue is T typedLocal)
            {
                return typedLocal;
            }

            if (GlobalVariables.TryGetValue(name, out var globalValue) && globalValue is T typedGlobal)
            {
                return typedGlobal;
            }

            return default;
        }

        /// <summary>
        /// 设置本地变量
        /// </summary>
        public void SetLocalVariable(string name, object? value)
        {
            LocalVariables[name] = value;
        }

        /// <summary>
        /// 设置全局变量
        /// </summary>
        public void SetGlobalVariable(string name, object? value)
        {
            GlobalVariables[name] = value;
        }
    }

    /// <summary>
    /// 日志级别
    /// </summary>
    public enum LogLevel
    {
        Debug,
        Info,
        Warning,
        Error
    }
}
