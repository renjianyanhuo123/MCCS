namespace MCCS.Events.StationSites.ControlChannels
{
    public record NotificationUpdateControlChannelEventParam
    {
        public long ControlChannelId { get; init; }
    }
}
