using MCCS.Infrastructure.Models.Devices;
using MCCS.Infrastructure.Models.StationSites;

namespace MCCS.Infrastructure.Domain.StationSites
{
    public class StationSiteAggregate
    {
        public required StationSiteInfo StationSiteInfo { get; set; } 
        public List<ControlChannelBindSignalInfo> ControlChannelSignalInfos { get; set; } = []; 
        public List<PseudoChannelBindSignalInfo> PseudoChannelInfos { get; set; } = [];
        /// <summary>
        /// 所有的绑定的信号接口
        /// </summary>
        public List<SignalInterfaceInfo> Signals { get; set; } = []; 

        public Model3DAggregate? Model3DAggregate { get; set; } 
    }
}
