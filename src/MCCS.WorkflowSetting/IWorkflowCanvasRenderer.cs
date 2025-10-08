using MCCS.WorkflowSetting.Models.Nodes;
using System.Windows.Controls;

namespace MCCS.WorkflowSetting
{
    public interface IWorkflowCanvasRenderer
    {
        void Initialize(Canvas canvas);
        /// <summary>
        /// 渲染整个流程图
        /// </summary>
        /// <param name="graph"></param>
        void RenderWorkflow(WorkflowGraph graph);
        /// <summary>
        /// 添加处理节点渲染(已经有原始画布了)
        /// </summary>
        /// <param name="node"></param>
        void AddProcessNodeRender(BaseNode node);
    }
}
