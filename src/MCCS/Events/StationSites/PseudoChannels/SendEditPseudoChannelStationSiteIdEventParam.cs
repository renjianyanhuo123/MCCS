namespace MCCS.Events.StationSites.PseudoChannels
{
    public record SendEditPseudoChannelStationSiteIdEventParam
    {
        public long StationId { get; init; }
        public long ChannelId { get; init; }
    }
}
