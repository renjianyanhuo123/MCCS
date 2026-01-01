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
        /// 将工作流节点树序列化为JSON字符串
        /// </summary>
        /// <param name="rootNode">根节点（主流程）</param>
        /// <param name="workflowName">工作流名称</param>
        /// <returns>JSON字符串</returns>
        string SerializeToJson(StepListNodes rootNode, string workflowName = "");

        /// <summary>
        /// 将工作流节点树序列化为WorkflowDto对象
        /// </summary>
        /// <param name="rootNode">根节点（主流程）</param>
        /// <param name="workflowName">工作流名称</param>
        /// <returns>WorkflowDto对象</returns>
        WorkflowDto SerializeToDto(StepListNodes rootNode, string workflowName = "");

        /// <summary>
        /// 从JSON字符串反序列化工作流
        /// </summary>
        /// <param name="json">JSON字符串</param>
        /// <param name="eventAggregator">事件聚合器</param>
        /// <param name="dialogService">对话框服务（用于DecisionNode）</param>
        /// <returns>重建的根节点</returns>
        StepListNodes DeserializeFromJson(string json, IEventAggregator eventAggregator, IDialogService? dialogService = null);

        /// <summary>
        /// 从WorkflowDto对象反序列化工作流
        /// </summary>
        /// <param name="workflowDto">WorkflowDto对象</param>
        /// <param name="eventAggregator">事件聚合器</param>
        /// <param name="dialogService">对话框服务（用于DecisionNode）</param>
        /// <returns>重建的根节点</returns>
        StepListNodes DeserializeFromDto(WorkflowDto workflowDto, IEventAggregator eventAggregator, IDialogService? dialogService = null);
    }
}
