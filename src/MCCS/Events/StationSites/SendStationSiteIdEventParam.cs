namespace MCCS.Events.StationSites
{
    public record SendStationSiteIdEventParam
    {
        public long StationId { get; init; }
    }
}
