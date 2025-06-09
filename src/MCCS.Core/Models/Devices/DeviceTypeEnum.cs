using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCCS.Core.Models.Devices
{
    public enum DeviceTypeEnum: int
    {
        Unknown = 0,
        // 传感器
        Sensor = 1,
        // 作动器
        Actuator = 2,
        // 控制器
        Controller = 3,
        // 网关
        Gateway = 4,
        // 服务器
        Display = 6,
        // 其他
        Other = 99
    }
}
