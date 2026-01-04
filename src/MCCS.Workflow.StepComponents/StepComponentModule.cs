using MCCS.Workflow.StepComponents.Core;
using MCCS.Workflow.StepComponents.Registry;
using MCCS.Workflow.StepComponents.Serialization;
using MCCS.Workflow.StepComponents.Steps;
using MCCS.Workflow.StepComponents.Workflows;
using Microsoft.Extensions.DependencyInjection;
using WorkflowCore.Interface;

namespace MCCS.Workflow.StepComponents
{
    /// <summary>
    /// 工作流步骤组件模块 - 基于 WorkflowCore
    /// </summary>
    public class StepComponentModule : IModule
    {
        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            // 注册步骤注册表为单例
            containerRegistry.RegisterSingleton<IStepRegistry, StepRegistry>();

            // 注册工作流服务
            containerRegistry.Register<IWorkflowService, WorkflowService>();

            // 注册组件序列化器
            containerRegistry.Register<IWorkflowSerializer, WorkflowSerializer>();

            // 注册 WorkflowCore 服务
            RegisterWorkflowCore(containerRegistry);
        }

        public void OnInitialized(IContainerProvider containerProvider)
        {
            // 获取步骤注册表
            var registry = containerProvider.Resolve<IStepRegistry>();

            if (registry is StepRegistry stepRegistry)
            {
                // 注册内置步骤
                RegisterBuiltInSteps(stepRegistry);

                // 自动发现并注册当前程序集中的所有步骤
                stepRegistry.DiscoverAndRegisterFromCurrentAssembly();
            } 
        }

        /// <summary>
        /// 注册 WorkflowCore 服务
        /// </summary>
        private static void RegisterWorkflowCore(IContainerRegistry containerRegistry)
        {
            // 创建服务集合用于 WorkflowCore
            var services = new ServiceCollection();

            // 添加 WorkflowCore 服务
            services.AddWorkflow();

            // 注册所有步骤类型
            services.AddTransient<DelayStep>();
            services.AddTransient<LogStep>();
            services.AddTransient<SetVariableStep>();
            services.AddTransient<ConditionStep>();
            services.AddTransient<HttpRequestStep>();
            services.AddTransient<MessageBoxStep>();

            // 注册 WorkflowCore 的主要服务到 Prism 容器
            //containerRegistry.RegisterInstance(serviceProvider.GetRequiredService<IWorkflowHost>());
            //containerRegistry.RegisterInstance(serviceProvider.GetRequiredService<IWorkflowRegistry>());
            //containerRegistry.RegisterInstance(serviceProvider.GetRequiredService<IWorkflowController>());
        }

        /// <summary>
        /// 注册内置步骤
        /// </summary>
        private static void RegisterBuiltInSteps(StepRegistry registry)
        {
            // 流程控制步骤
            registry.RegisterStep<DelayStep>();
            registry.RegisterStep<ConditionStep>();
            // 通用步骤
            registry.RegisterStep<LogStep>();
            // 数据处理步骤
            registry.RegisterStep<SetVariableStep>();
            // 网络步骤
            registry.RegisterStep<HttpRequestStep>();
            // 用户交互步骤
            registry.RegisterStep<MessageBoxStep>();
        }
    }
}
