using MCCS.Core.Models.Devices;
using MCCS.Core.Models.StationSites;

namespace MCCS.Core.Domain.StationSites
{
    public class PseudoChannelBindSignalInfo
    {
        public PseudoChannelInfo PseudoChannelInfo { get; set; }

        public List<SignalInterfaceInfo> Signals { get; set; }
    } 
}
