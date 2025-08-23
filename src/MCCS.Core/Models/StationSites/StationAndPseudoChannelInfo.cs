using FreeSql.DataAnnotations;

namespace MCCS.Core.Models.StationSites
{
    [Table(Name = "tb_stationAndPseudoChannelInfo")]
    public class StationAndPseudoChannelInfo : BaseModel
    {
        public long StationId { get; set; }

        public long PseudoChanndleId { get; set; }
    }
}
