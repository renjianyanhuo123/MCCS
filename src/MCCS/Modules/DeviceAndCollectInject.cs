using MCCS.Core.Devices;
using MCCS.Services.DevicesService;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCCS.Modules
{
    internal static class DeviceAndCollectInject
    {
        internal static void Inject(this IContainerRegistry containerRegistry, IConfiguration configuration)
        {
            // 设备连接工厂
            containerRegistry.RegisterSingleton<IDeviceConnectionFactory, DeviceConnectionFactory>();
            // 设备创建工厂
            containerRegistry.RegisterSingleton<IDeviceFactory, DeviceFactory>();
            // 设备管理注入---全局单例
            containerRegistry.RegisterSingleton<IDeviceManager, DeviceManager>();
            // 设备服务注入---全局单例
            // MCCS.Core.Devices.Manager.DeviceManager.Inject();
            // 设备指令
            // MCCS.Core.Devices.Commands.CommandManager.Inject();
            // 设备系统协调器
            // MCCS.Services.Coordinators.IDeviceSystemCoordinator.Inject();
        }
    }
}
