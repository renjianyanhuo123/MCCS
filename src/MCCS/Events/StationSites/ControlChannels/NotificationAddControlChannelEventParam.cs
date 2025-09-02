namespace MCCS.Events.StationSites.ControlChannels
{
    public record NotificationAddControlChannelEventParam
    {
        public long ControlChannelId { get; init; }
    }
}
