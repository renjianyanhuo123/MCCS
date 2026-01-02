
using MCCS.WorkflowSetting.Components;
using MCCS.WorkflowSetting.Models.Nodes;
using MCCS.WorkflowSetting.Serialization;

namespace MCCS.WorkflowSetting
{
    public class WorkflowModule : IModule
    {
        /// <summary>
        /// (2)初始化操作
        /// </summary>
        /// <param name="containerProvider"></param>
        public void OnInitialized(IContainerProvider containerProvider)
        {
            // No initialization logic required at this time
        }
        /// <summary>
        /// (1)注册类型
        /// </summary>
        /// <param name="containerRegistry"></param>
        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            // 指定ViewModel创建
            // containerRegistry.RegisterForNavigation<WorkflowStepListNodes, StepListNodes>(); 
            // 注册序列化服务
            containerRegistry.Register<IWorkflowSerializer, WorkflowSerializer>(); 
        }
    }
}
