using MCCS.Core.Devices.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCCS.Events.ControlCommand
{
    public record NotificationCommandFinishedEventParam
    {
        public required string CommandId { get; set; }

        public CommandExecuteStatusEnum CommandExecuteStatus { get; set; }
    }
}
