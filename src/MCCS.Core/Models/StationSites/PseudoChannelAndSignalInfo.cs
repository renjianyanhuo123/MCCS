using FreeSql.DataAnnotations;

namespace MCCS.Core.Models.StationSites
{
    [Table(Name = "stationSite_PseudoChannelAndSignalInfo")]
    public class PseudoChannelAndSignalInfo : BaseModel
    {
        /// <summary>
        /// 信号ID
        /// </summary>
        public long SignalId { get; set; }
        /// <summary>
        /// 虚拟通道ID
        /// </summary>
        public long PseudoChannelId { get; set; }
    }
}
