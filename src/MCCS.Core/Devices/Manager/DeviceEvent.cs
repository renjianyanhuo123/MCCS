using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCCS.Core.Devices.Manager
{
    /// <summary>
    /// 设备事件基类
    /// </summary>
    public abstract class DeviceEvent
    {
        public string DeviceId { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
