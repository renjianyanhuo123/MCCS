using MCCS.Core.Models.StationSites;

namespace MCCS.Core.Domain.StationSites
{
    public class StationSiteAggregate
    {
        public StationSiteInfo StationSiteInfo { get; set; }

        public List<ControlChannelInfo> ControlChannelInfos { get; set; } = [];

        public List<PseudoChannelInfo> PseudoChannelInfos { get; set; } = [];
    }
}
