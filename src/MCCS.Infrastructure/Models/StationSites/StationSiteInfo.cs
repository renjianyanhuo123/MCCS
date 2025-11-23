using FreeSql.DataAnnotations;

namespace MCCS.Infrastructure.Models.StationSites
{
    /// <summary>
    /// 站点信息
    /// </summary>
    [Table(Name = "tb_stationSiteInfo")]
    public class StationSiteInfo : BaseModel
    {
        [Column(IsNullable = false, StringLength = 100)]
        public required string StationName { get; set; }

        [Column(IsNullable = true, StringLength = 500)]
        public string Description { get; set; } = string.Empty;
        /// <summary>
        /// 是否正在使用
        /// </summary>
        public bool IsUsing { get; set; }
    }
}
