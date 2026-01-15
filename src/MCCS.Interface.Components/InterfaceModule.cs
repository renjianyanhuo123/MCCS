using MCCS.Interface.Components.Registry;
using MCCS.Interface.Components.ViewModels;
using MCCS.Interface.Components.ViewModels.Parameters;
using MCCS.Interface.Components.Views;

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
            containerRegistry.RegisterSingleton<IInterfaceRegistry, InterfaceRegistry>();
            // 组件列表界面注册
            containerRegistry.RegisterForNavigation<MethodComponentsPage>(nameof(MethodComponentsPageViewModel));
            // 组件参数配置界面注册
            containerRegistry.RegisterForNavigation<MethodChartSetParamPage>(nameof(MethodChartSetParamPageViewModel));
            containerRegistry.RegisterForNavigation<DataMonitorSetParamPage>(nameof(DataMonitorSetParamPageViewModel));
            // 注册控制命令页面 
            //containerRegistry.RegisterForNavigation<ViewManualControl>(nameof(ViewManualControl));
            //containerRegistry.RegisterForNavigation<ViewStaticControl>(nameof(ViewStaticControl));
            //containerRegistry.RegisterForNavigation<ViewProgramControl>(nameof(ViewProgramControl));
            //containerRegistry.RegisterForNavigation<ViewFatigueControl>(nameof(ViewFatigueControl));
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
