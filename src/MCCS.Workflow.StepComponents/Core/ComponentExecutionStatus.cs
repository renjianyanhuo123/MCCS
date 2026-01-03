namespace MCCS.Workflow.StepComponents.Core
{
    /// <summary>
    /// 组件执行状态
    /// </summary>
    public enum ComponentExecutionStatus
    {
        /// <summary>
        /// 未开始
        /// </summary>
        NotStarted,

        /// <summary>
        /// 执行中
        /// </summary>
        Running,

        /// <summary>
        /// 执行成功
        /// </summary>
        Success,

        /// <summary>
        /// 执行失败
        /// </summary>
        Failed,

        /// <summary>
        /// 已取消
        /// </summary>
        Cancelled,

        /// <summary>
        /// 已跳过
        /// </summary>
        Skipped
    }
}
