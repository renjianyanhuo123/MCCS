using MCCS.Common.Resources.Extensions;
using MCCS.Components.LayoutRootComponents;
using MCCS.Services.ProjectServices;

using Microsoft.Extensions.Configuration; 

namespace MCCS.Modules
{
    internal static class DeviceAndCollectInject
    {
        internal static void Inject(this IContainerRegistry containerRegistry, IConfiguration configuration)
        {
            // 注入协调管理
            // containerRegistry.RegisterSingleton<IDataCollector, DataCollector>(); 
            containerRegistry.Register<IDialogService, MaterialDialogService>();
            containerRegistry.Register<ILayoutTreeTraversal, LayoutTreeTraversal>();
            containerRegistry.Register<IProjectComponentFactoryService, ProjectComponentFactoryService>();
        }
    }
}
