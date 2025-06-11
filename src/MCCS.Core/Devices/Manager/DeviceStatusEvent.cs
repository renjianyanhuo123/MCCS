using MCCS.Core.Models.Devices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCCS.Core.Devices.Manager
{
    /// <summary>
    /// 设备状态变化事件
    /// </summary>
    public class DeviceStatusEvent : DeviceEvent
    {
        public DeviceStatusEnum OldStatus { get; set; }
        public DeviceStatusEnum NewStatus { get; set; }
    }
}
