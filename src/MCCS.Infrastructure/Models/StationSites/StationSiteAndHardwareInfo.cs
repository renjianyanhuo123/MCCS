using FreeSql.DataAnnotations;

namespace MCCS.Infrastructure.Models.StationSites
{
    [Table(Name = "tb_controlChannelAndHardwareInfo")]
    public class StationSiteAndHardwareInfo : BaseModel
    {
        public long StationId { get; set; }
        public long HardwareId { get; set; }
        public long SignalId { get; set; }
    }
}
