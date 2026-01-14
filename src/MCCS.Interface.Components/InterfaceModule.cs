using MCCS.Interface.Components.Registry;
using MCCS.Interface.Components.ViewModels;
using MCCS.Interface.Components.ViewModels.Parameters;
using MCCS.Interface.Components.Views;
using MCCS.Interface.Components.Views.ControlCommandPages;

namespace MCCS.Interface.Components
{
    /// <summary>
    /// 界面组件模块 - 负责注册和管理所有界面组件
    /// </summary>
    public class InterfaceModule : IModule
    {
        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            // 注册界面组件注册表为单例（使用工厂方法以便传入容器解析器）
            containerRegistry.RegisterSingleton<IInterfaceRegistry>(container =>
            {
                // 创建一个解析器函数，用于从 Prism 容器中获取服务
                Func<Type, object> resolver = type => container.Resolve(type);
                return new InterfaceRegistry(resolver);
            });

            // 注册页面导航
            containerRegistry.RegisterForNavigation<ProjectChartComponentPage>(nameof(ProjectChartComponentPageViewModel));
            containerRegistry.RegisterForNavigation<ProjectDataMonitorComponentPage>(nameof(ProjectDataMonitorComponentPageViewModel));
            containerRegistry.RegisterForNavigation<MethodChartSetParamPage>(nameof(MethodChartSetParamPageViewModel));
            containerRegistry.RegisterForNavigation<DataMonitorSetParamPage>(nameof(DataMonitorSetParamPageViewModel));
            containerRegistry.RegisterForNavigation<ControlOperationComponentPage>(nameof(ControlOperationComponentPage));

            // 注册控制命令页面
            containerRegistry.RegisterForNavigation<ViewManualControl>(nameof(ViewManualControl));
            containerRegistry.RegisterForNavigation<ViewStaticControl>(nameof(ViewStaticControl));
            containerRegistry.RegisterForNavigation<ViewProgramControl>(nameof(ViewProgramControl));
            containerRegistry.RegisterForNavigation<ViewFatigueControl>(nameof(ViewFatigueControl));
        }

        public void OnInitialized(IContainerProvider containerProvider)
        {
            // 获取界面组件注册表
            var registry = containerProvider.Resolve<IInterfaceRegistry>();

            if (registry is InterfaceRegistry interfaceRegistry)
            {
                // 自动发现并注册当前程序集中的所有界面组件
                interfaceRegistry.DiscoverAndRegisterFromCurrentAssembly();
            }
        }
    }
}
