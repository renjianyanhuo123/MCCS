using MCCS.WorkflowSetting.Models.Nodes;

namespace MCCS.WorkflowSetting
{
    /// <summary>
    /// 工作流管理器接口
    /// 负责工作流数据的序列化、反序列化和状态管理
    /// 注意：此接口只负责数据层面的操作，不涉及UI层面的Canvas操作
    /// UI绑定应该由ViewModel或View层处理
    /// </summary>
    public interface ICanvasManager
    {
        /// <summary>
        /// 从JSON字符串加载并还原工作流
        /// </summary>
        /// <param name="json">工作流JSON字符串</param>
        /// <returns>还原的工作流根节点</returns>
        StepListNodes LoadWorkflowFromJson(string json);

        /// <summary>
        /// 保存当前工作流为JSON字符串
        /// </summary>
        /// <param name="workflowName">工作流名称</param>
        /// <returns>JSON字符串</returns>
        string SaveWorkflowToJson(string workflowName = "");

        /// <summary>
        /// 保存当前工作流到文件
        /// </summary>
        /// <param name="filePath">文件路径</param>
        /// <param name="workflowName">工作流名称</param>
        Task SaveWorkflowToFileAsync(string filePath, string workflowName = "");

        /// <summary>
        /// 从文件加载工作流
        /// </summary>
        /// <param name="filePath">文件路径</param>
        /// <returns>加载的工作流根节点</returns>
        Task<StepListNodes> LoadWorkflowFromFileAsync(string filePath);

        /// <summary>
        /// 设置当前工作流根节点（用于保存操作）
        /// </summary>
        /// <param name="rootNode">根节点</param>
        void SetWorkflowRoot(StepListNodes rootNode);

        /// <summary>
        /// 获取当前工作流根节点
        /// </summary>
        StepListNodes? GetWorkflowRoot();
    }
}
