using MCCS.Workflow.StepComponents.Core;

namespace MCCS.Workflow.StepComponents.Workflows
{
    /// <summary>
    /// 工作流服务接口
    /// </summary>
    public interface IWorkflowService
    {
        /// <summary>
        /// 注册工作流定义
        /// </summary>
        void RegisterWorkflow(WorkflowDefinition definition);

        /// <summary>
        /// 启动工作流
        /// </summary>
        Task<string> StartWorkflowAsync(string workflowId, WorkflowStepData? initialData = null);

        /// <summary>
        /// 启动工作流（使用定义）
        /// </summary>
        Task<string> StartWorkflowAsync(WorkflowDefinition definition, WorkflowStepData? initialData = null);

        /// <summary>
        /// 暂停工作流
        /// </summary>
        Task<bool> SuspendWorkflowAsync(string workflowInstanceId);

        /// <summary>
        /// 恢复工作流
        /// </summary>
        Task<bool> ResumeWorkflowAsync(string workflowInstanceId);

        /// <summary>
        /// 终止工作流
        /// </summary>
        Task<bool> TerminateWorkflowAsync(string workflowInstanceId);

        /// <summary>
        /// 获取工作流状态
        /// </summary>
        Task<WorkflowInstanceInfo?> GetWorkflowStatusAsync(string workflowInstanceId);

        /// <summary>
        /// 发布事件（用于等待步骤）
        /// </summary>
        Task PublishEventAsync(string eventName, string eventKey, object? eventData = null);

        /// <summary>
        /// 工作流完成事件
        /// </summary>
        event EventHandler<WorkflowCompletedEventArgs>? WorkflowCompleted;

        /// <summary>
        /// 步骤完成事件
        /// </summary>
        event EventHandler<StepCompletedEventArgs>? StepCompleted;

        /// <summary>
        /// 工作流错误事件
        /// </summary>
        event EventHandler<WorkflowErrorEventArgs>? WorkflowError;
    }

    /// <summary>
    /// 工作流实例信息
    /// </summary>
    public class WorkflowInstanceInfo
    {
        public string InstanceId { get; set; } = string.Empty;
        public string WorkflowId { get; set; } = string.Empty;
        public int Version { get; set; }
        public WorkflowStatus Status { get; set; }
        public DateTime CreateTime { get; set; }
        public DateTime? CompleteTime { get; set; }
        public WorkflowStepData? Data { get; set; }
    }

    /// <summary>
    /// 工作流完成事件参数
    /// </summary>
    public class WorkflowCompletedEventArgs : EventArgs
    {
        public string WorkflowInstanceId { get; set; } = string.Empty;
        public string WorkflowId { get; set; } = string.Empty;
        public WorkflowStatus Status { get; set; }
        public WorkflowStepData? Data { get; set; }
        public TimeSpan Duration { get; set; }
    }

    /// <summary>
    /// 步骤完成事件参数
    /// </summary>
    public class StepCompletedEventArgs : EventArgs
    {
        public string WorkflowInstanceId { get; set; } = string.Empty;
        public string StepId { get; set; } = string.Empty;
        public string StepName { get; set; } = string.Empty;
        public StepExecutionStatus Status { get; set; }
        public StepOutputData? Output { get; set; }
    }

    /// <summary>
    /// 工作流错误事件参数
    /// </summary>
    public class WorkflowErrorEventArgs : EventArgs
    {
        public string WorkflowInstanceId { get; set; } = string.Empty;
        public string? StepId { get; set; }
        public string ErrorMessage { get; set; } = string.Empty;
        public Exception? Exception { get; set; }
    }
}
