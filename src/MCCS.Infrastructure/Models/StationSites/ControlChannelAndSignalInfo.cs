using FreeSql.DataAnnotations;

namespace MCCS.Infrastructure.Models.StationSites
{
    [Table(Name = "tb_controlChannelAndSignalInfo")]
    public class ControlChannelAndSignalInfo : BaseModel
    {
        public long ChannelId { get; set; }
        public long SignalId { get; set; }
        public long DeviceId { get; set; }
        public SignalTypeEnum SignalType { get; set; }
    }
}
