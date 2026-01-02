using MCCS.WorkflowSetting.Models.Nodes;
using MCCS.WorkflowSetting.Serialization.Dtos;

namespace MCCS.WorkflowSetting.Serialization
{
    /// <summary>
    /// 工作流序列化服务接口
    /// </summary>
    public interface IWorkflowSerializer
    {
        /// <summary>
        /// 将StepListNodes转换为可序列化的DTO对象，并序列化为JSON字符串
        /// </summary>
        /// <param name="stepListNodes">工作流节点</param>
        /// <returns>序列化后的JSON字符串</returns>
        string Serialize(StepListNodes stepListNodes);

        /// <summary>
        /// 将StepListNodes转换为DTO对象（不序列化为字符串）
        /// </summary>
        /// <param name="stepListNodes">工作流节点</param>
        /// <returns>工作流DTO对象</returns>
        WorkflowDto ToDto(StepListNodes stepListNodes);

        /// <summary>
        /// 将JSON字符串反序列化为StepListNodes
        /// </summary>
        /// <param name="json">JSON字符串</param>
        /// <param name="eventAggregator">事件聚合器（用于重建节点）</param>
        /// <param name="dialogService">对话框服务（用于DecisionNode）</param>
        /// <returns>反序列化后的StepListNodes</returns>
        StepListNodes Deserialize(string json, IEventAggregator eventAggregator, IDialogService dialogService);

        /// <summary>
        /// 将DTO对象转换为StepListNodes（不需要从字符串反序列化）
        /// </summary>
        /// <param name="workflowDto">工作流DTO对象</param>
        /// <param name="eventAggregator">事件聚合器（用于重建节点）</param>
        /// <param name="dialogService">对话框服务（用于DecisionNode）</param>
        /// <returns>转换后的StepListNodes</returns>
        StepListNodes FromDto(WorkflowDto workflowDto, IEventAggregator eventAggregator, IDialogService dialogService);
    }
}
