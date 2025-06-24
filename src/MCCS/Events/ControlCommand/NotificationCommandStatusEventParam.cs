using MCCS.Core.Devices.Commands;

namespace MCCS.Events.ControlCommand
{
    public record NotificationCommandStatusEventParam
    {
        public required string CommandId { get; set; }

        public CommandExecuteStatusEnum CommandExecuteStatus { get; set; }
    }
}
