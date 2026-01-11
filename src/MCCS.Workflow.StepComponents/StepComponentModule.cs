using MCCS.Workflow.StepComponents.Core;
using MCCS.Workflow.StepComponents.Registry;
using MCCS.Workflow.StepComponents.Serialization;
using MCCS.Workflow.StepComponents.Steps;
using MCCS.Workflow.StepComponents.Steps.StructuralTest;
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

                // 注册结构试验步骤
                RegisterStructuralTestSteps(stepRegistry);

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

            // 注册通用步骤类型
            services.AddTransient<DelayStep>();
            services.AddTransient<LogStep>();
            services.AddTransient<SetVariableStep>();
            services.AddTransient<ConditionStep>();

            // 注册结构试验步骤类型
            // A. 基础与安全
            services.AddTransient<LoadRecipeStep>();
            services.AddTransient<ConnectDevicesStep>();
            services.AddTransient<SafetyInterlockCheckStep>();
            services.AddTransient<EnableControllerStep>();

            // B. 校准与核查
            services.AddTransient<VerifyForceChainStep>();
            services.AddTransient<VerifyExtensometerStep>();

            // C. 人工操作与基线
            services.AddTransient<UserMountSpecimenStep>();
            services.AddTransient<ZeroSensorsStep>();
            services.AddTransient<PreloadStep>();

            // D. 核心控制执行
            services.AddTransient<ExecuteSegmentStep>();
            services.AddTransient<ExecuteCyclicSegmentStep>();
            services.AddTransient<ExecuteHoldMonitorStep>();
            services.AddTransient<EvaluateStopCriteriaStep>();
            services.AddTransient<UnloadToSafeStep>();

            // E. 数据与报告
            services.AddTransient<StartAcquisitionStep>();
            services.AddTransient<StopAcquisitionStep>();
            services.AddTransient<GenerateReportStep>();
        }

        /// <summary>
        /// 注册内置通用步骤
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
        }

        /// <summary>
        /// 注册结构试验步骤
        /// </summary>
        private static void RegisterStructuralTestSteps(StepRegistry registry)
        {
            // A. 基础与安全类
            registry.RegisterStep<LoadRecipeStep>();
            registry.RegisterStep<ConnectDevicesStep>();
            registry.RegisterStep<SafetyInterlockCheckStep>();
            registry.RegisterStep<EnableControllerStep>();

            // B. 校准与核查类
            registry.RegisterStep<VerifyForceChainStep>();
            registry.RegisterStep<VerifyExtensometerStep>();

            // C. 人工操作与基线类
            registry.RegisterStep<UserMountSpecimenStep>();
            registry.RegisterStep<ZeroSensorsStep>();
            registry.RegisterStep<PreloadStep>();

            // D. 核心控制段执行类
            registry.RegisterStep<ExecuteSegmentStep>();
            registry.RegisterStep<ExecuteCyclicSegmentStep>();
            registry.RegisterStep<ExecuteHoldMonitorStep>();
            registry.RegisterStep<EvaluateStopCriteriaStep>();
            registry.RegisterStep<UnloadToSafeStep>();

            // E. 数据与报告类
            registry.RegisterStep<StartAcquisitionStep>();
            registry.RegisterStep<StopAcquisitionStep>();
            registry.RegisterStep<GenerateReportStep>();
        }
    }
}
