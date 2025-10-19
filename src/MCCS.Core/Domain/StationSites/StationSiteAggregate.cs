using MCCS.Core.Models.Devices;
using MCCS.Core.Models.StationSites;

namespace MCCS.Core.Domain.StationSites
{
    public class StationSiteAggregate
    {
        public StationSiteInfo StationSiteInfo { get; set; } 
        public List<ControlChannelBindSignalInfo> ControlChannelSignalInfos { get; set; } = []; 
        public List<PseudoChannelInfo> PseudoChannelInfos { get; set; } = [];
        /// <summary>
        /// 所有的绑定的信号接口
        /// </summary>
        public List<SignalInterfaceInfo> Signals { get; set; } = [];
        public Model3DAggregate? Model3DAggregate { get; set; } 
    }
}
