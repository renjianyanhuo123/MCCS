namespace MCCS.Events.StationSites.ControlChannels
{
    public record SendEditChannelStationSiteIdEventParam
    {
        public long StationId { get; init; }
        public long ChannelId { get; init; }
    }
}
