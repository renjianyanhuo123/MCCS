using MCCS.Common.DataManagers;
using MCCS.Common.Resources.Extensions;
using MCCS.Components.LayoutRootComponents;
using MCCS.Infrastructure.Helper;
using MCCS.Services.ProjectServices;
using MCCS.Services.StationServices;
using MCCS.Station.Abstractions.Interfaces;

using Microsoft.Extensions.Configuration; 

namespace MCCS.Modules
{
    internal static class DeviceAndCollectInject
    {
        internal static void Inject(this IContainerRegistry containerRegistry, IConfiguration configuration)
        {
            // 设置站点采集进程运行
            var isMock = configuration["AppSettings:IsMock"] ?? "true";
            GlobalDataManager.Instance.SetValue(new ProcessManager("MCCS.Station.Host.exe", isMock, AppContext.BaseDirectory, false));
            // 注入协调管理
            // containerRegistry.RegisterSingleton<IDataCollector, DataCollector>(); 
            containerRegistry.Register<IDialogService, MaterialDialogService>();
            containerRegistry.Register<IStationService, StationService>();
            containerRegistry.Register<ILayoutTreeTraversal, LayoutTreeTraversal>(); 
        }
    }
}
