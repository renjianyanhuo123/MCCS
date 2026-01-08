using MCCS.Infrastructure.Models.StationSites;

namespace MCCS.Infrastructure.Domain.StationSites
{
    public class PseudoChannelBindSignalInfo
    {
        public required PseudoChannelInfo PseudoChannelInfo { get; set; }

        public List<long> SignalIds { get; set; } = [];
    } 
}
