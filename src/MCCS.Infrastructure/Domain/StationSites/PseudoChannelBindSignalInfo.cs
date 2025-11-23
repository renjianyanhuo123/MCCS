using MCCS.Infrastructure.Models.Devices;
using MCCS.Infrastructure.Models.StationSites;

namespace MCCS.Infrastructure.Domain.StationSites
{
    public class PseudoChannelBindSignalInfo
    {
        public PseudoChannelInfo PseudoChannelInfo { get; set; }

        public List<SignalInterfaceInfo> Signals { get; set; }
    } 
}
