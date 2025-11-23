using FreeSql.DataAnnotations;

namespace MCCS.Infrastructure.Models.StationSites
{
    [Table(Name = "stationSite_hydraulicOilInfo")]
    public class HydraulicOilInfo : BaseModel
    {
        /// <summary>
        /// 油源名称
        /// </summary>
        [Column(StringLength = 500)]
        public string Name { get; set; } = string.Empty;
        /// <summary>
        /// 额定压力上限
        /// </summary>
        public double RatedPressureUpLimit { get; set; }
        /// <summary>
        /// 额定压力下限
        /// </summary>
        public double RatedPressureDownLimit { get; set; }
    }
}
