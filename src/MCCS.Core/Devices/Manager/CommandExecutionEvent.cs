using MCCS.Core.Devices.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCCS.Core.Devices.Manager
{
    /// <summary>
    /// 指令执行事件
    /// </summary>
    public class CommandExecutionEvent : DeviceEvent
    {
        public CommandResponse? Response { get; set; }
    }
}
