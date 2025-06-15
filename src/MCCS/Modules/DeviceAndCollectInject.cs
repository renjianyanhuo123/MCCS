using MCCS.Core.Devices;  
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks; 
using MCCS.Core.Devices.Manager;
using MCCS.Core.Devices.Collections;
using MCCS.Core.Devices.Connections;

namespace MCCS.Modules
{
    internal static class DeviceAndCollectInject
    {
        internal static void Inject(this IContainerRegistry containerRegistry, IConfiguration configuration)
        {
            // 设备连接工厂
            containerRegistry.RegisterSingleton<IDeviceConnectionFactory, DeviceConnectionFactory>();
            // 
            containerRegistry.RegisterSingleton<IConnectionManager, ConnectionManager>();
            // 设备创建工厂
            containerRegistry.RegisterSingleton<IDeviceFactory, DeviceFactory>();
            // 设备管理注入---全局单例
            containerRegistry.RegisterSingleton<IDeviceManager, DeviceManager>(); 
            // 注入协调管理
            // containerRegistry.RegisterSingleton<IDataCollector, DataCollector>();
        }
    }
}
