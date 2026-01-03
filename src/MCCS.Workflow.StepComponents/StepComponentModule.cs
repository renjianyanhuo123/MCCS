using MCCS.Workflow.StepComponents.Components;
using MCCS.Workflow.StepComponents.Registry;
using MCCS.Workflow.StepComponents.Serialization;

namespace MCCS.Workflow.StepComponents
{
    /// <summary>
    /// 工作流组件模块
    /// </summary>
    public class StepComponentModule : IModule
    {
        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            // 注册组件注册表为单例
            containerRegistry.RegisterSingleton<IComponentRegistry, ComponentRegistry>();

            // 注册组件序列化器
            containerRegistry.Register<IComponentSerializer, ComponentSerializer>();
        }

        public void OnInitialized(IContainerProvider containerProvider)
        {
            // 获取组件注册表
            var registry = containerProvider.Resolve<IComponentRegistry>();

            if (registry is ComponentRegistry componentRegistry)
            {
                // 注册内置组件
                RegisterBuiltInComponents(componentRegistry);

                // 自动发现并注册当前程序集中的所有组件
                componentRegistry.DiscoverAndRegisterFromCurrentAssembly();
            }
        }

        /// <summary>
        /// 注册内置组件
        /// </summary>
        private static void RegisterBuiltInComponents(ComponentRegistry registry)
        {
            // 流程控制组件
            registry.Register<DelayComponent>();
            registry.Register<ConditionComponent>();

            // 通用组件
            registry.Register<LogComponent>();

            // 数据处理组件
            registry.Register<SetVariableComponent>();

            // 网络组件
            registry.Register<HttpRequestComponent>();

            // 用户交互组件
            registry.Register<MessageBoxComponent>();
        }
    }
}
