using MCCS.Workflow.StepComponents.Core;
using MCCS.Workflow.StepComponents.Workflows;

namespace MCCS.Workflow.StepComponents.Serialization
{
    /// <summary>
    /// 工作流序列化器接口
    /// </summary>
    public interface IWorkflowSerializer
    {
        /// <summary>
        /// 序列化工作流定义为JSON
        /// </summary>
        string SerializeWorkflow(WorkflowDefinition definition);

        /// <summary>
        /// 从JSON反序列化工作流定义
        /// </summary>
        WorkflowDefinition? DeserializeWorkflow(string json);

        /// <summary>
        /// 序列化步骤配置为JSON
        /// </summary>
        string SerializeStep(StepConfiguration step);

        /// <summary>
        /// 从JSON反序列化步骤配置
        /// </summary>
        StepConfiguration? DeserializeStep(string json);

        /// <summary>
        /// 序列化工作流数据为JSON
        /// </summary>
        string SerializeWorkflowData(WorkflowStepData data);

        /// <summary>
        /// 从JSON反序列化工作流数据
        /// </summary>
        WorkflowStepData? DeserializeWorkflowData(string json);

        /// <summary>
        /// 保存工作流定义到文件
        /// </summary>
        Task SaveWorkflowAsync(WorkflowDefinition definition, string filePath);

        /// <summary>
        /// 从文件加载工作流定义
        /// </summary>
        Task<WorkflowDefinition?> LoadWorkflowAsync(string filePath);
    }
}
