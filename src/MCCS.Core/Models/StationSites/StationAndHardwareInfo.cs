using FreeSql.DataAnnotations;

namespace MCCS.Core.Models.StationSites
{
    [Table(Name = "tb_stationAndHardwareInfo")]
    public class StationAndHardwareInfo
    {
        public long StationId { get; set; }
        public long DeviceId { get; set; }
    }
}
