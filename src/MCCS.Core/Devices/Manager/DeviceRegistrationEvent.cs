using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCCS.Core.Devices.Manager
{
    /// <summary>
    /// 设备注册事件
    /// </summary>
    public class DeviceRegistrationEvent : DeviceEvent
    {
        public RegistrationType Type { get; set; }
    }
}
