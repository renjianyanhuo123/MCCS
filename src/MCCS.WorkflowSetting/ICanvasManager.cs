using System.Windows.Controls;
using MCCS.WorkflowSetting.Models.Nodes;

namespace MCCS.WorkflowSetting
{
    public interface ICanvasManager
    {
        /// <summary>
        /// 初始化画布管理器
        /// </summary>
        /// <param name="canvas">画布控件</param>
        void Inititial(Canvas canvas);

        /// <summary>
        /// 从JSON字符串渲染工作流
        /// </summary>
        /// <param name="json">工作流JSON字符串</param>
        void RenderWorkflowByJson(string json);

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
        Task LoadWorkflowFromFileAsync(string filePath);

        /// <summary>
        /// 设置当前工作流根节点
        /// </summary>
        /// <param name="rootNode">根节点</param>
        void SetWorkflowRoot(StepListNodes rootNode);

        /// <summary>
        /// 获取当前工作流根节点
        /// </summary>
        StepListNodes? GetWorkflowRoot();

        /// <summary>
        /// 添加节点（保留用于未来扩展）
        /// </summary>
        void Add();
    }
}
