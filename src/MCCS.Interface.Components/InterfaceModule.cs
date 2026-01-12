using MCCS.Interface.Components.ViewModels;
using MCCS.Interface.Components.Views;

namespace MCCS.Interface.Components
{
    public class InterfaceModule : IModule
    {
        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterForNavigation<ProjectChartComponentPage>(nameof(ProjectChartComponentPageViewModel));
            containerRegistry.RegisterForNavigation<ProjectDataMonitorComponentPage>(nameof(ProjectChartComponentPageViewModel));
        }

        public void OnInitialized(IContainerProvider containerProvider)
        {

        }
    }
}
