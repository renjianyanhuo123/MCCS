using Microsoft.Extensions.Configuration; 

namespace MCCS.Modules
{
    internal static class DeviceAndCollectInject
    {
        internal static void Inject(this IContainerRegistry containerRegistry, IConfiguration configuration)
        { 
            // 注入协调管理
            // containerRegistry.RegisterSingleton<IDataCollector, DataCollector>();
        }
    }
}
