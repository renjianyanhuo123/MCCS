using MCCS.Core.Models.Devices;
using MCCS.Core.Models.StationSites;

namespace MCCS.Core.Domain.StationSites
{
    public class StationSiteAggregate
    {
        public StationSiteInfo StationSiteInfo { get; set; }

        public List<ControlChannelInfo> ControlChannelInfos { get; set; } = [];

        public List<PseudoChannelInfo> PseudoChannelInfos { get; set; } = [];

        public List<SignalInterfaceInfo> Signals { get; set; } = [];

        public Model3DAggregate? Model3DAggregate { get; set; }

        public List<ControlChannelAndSignalInfo> ControlChannelsAndSignalInfos { get; set;} = [];
    }
}
