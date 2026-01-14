using MCCS.Interface.Components.ViewModels;
using MCCS.Interface.Components.ViewModels.Parameters;
using MCCS.Interface.Components.Views;

namespace MCCS.Interface.Components
{
    public class InterfaceModule : IModule
    {
        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterForNavigation<ProjectChartComponentPage>(nameof(ProjectChartComponentPageViewModel));
            containerRegistry.RegisterForNavigation<ProjectDataMonitorComponentPage>(nameof(ProjectChartComponentPageViewModel));
            containerRegistry.RegisterForNavigation<MethodChartSetParamPage>(nameof(MethodChartSetParamPageViewModel));
            containerRegistry.RegisterForNavigation<DataMonitorSetParamPage>(nameof(DataMonitorSetParamPageViewModel));
        }

        public void OnInitialized(IContainerProvider containerProvider)
        {

        }
    }
}
