using FreeSql.DataAnnotations;

namespace MCCS.Core.Models.SystemManager
{
    [Table(Name = "tb_channelInfo")]
    public class ChannelInfo : BaseModel
    {
        /// <summary>
        /// 内部名称
        /// </summary>
        [Column(IsNullable = false, StringLength = 50)]
        public required string ChannelId { get; set; } = string.Empty;

        [Column(IsNullable = false, StringLength = 100)]
        public required string ChannelName { get; set; }

        /// <summary>
        /// 是否可见
        /// </summary>
        public bool IsShowable { get; set; }
        /// <summary>
        /// 是否开启试样保护
        /// </summary>
        public bool IsOpenSpecimenProtected { get; set; }
    }
}
