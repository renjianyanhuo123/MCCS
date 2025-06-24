using MCCS.Events.ControlCommand;

namespace MCCS.Events.Controllers
{
    public sealed class NotificationCommandStopedEvent : PubSubEvent<NotificationCommandStatusEventParam>
    {
    }
}
