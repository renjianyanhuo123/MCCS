using FreeSql.DataAnnotations;

namespace MCCS.Core.Models.StationSites
{
    /// <summary>
    /// 虚拟通道
    /// </summary>
    [Table(Name = "tb_pseudoChannelInfo")]
    public class PseudoChannelInfo : BaseModel
    {
        /// <summary>
        /// 内部名称
        /// </summary>
        [Column(IsNullable = false, StringLength = 50)]
        public required string ChannelId { get; set; } = string.Empty;

        [Column(IsNullable = false, StringLength = 100)]
        public required string ChannelName { get; set; }
    }
}
