using FreeSql.DataAnnotations;

namespace MCCS.Infrastructure.Models.StationSites
{
    [Table(Name = "tb_controlChannelAndModel3DInfo")]
    public class ControlChannelAndModel3DInfo : BaseModel
    {
        public long ControlChannelId { get; set; }
        /// <summary>
        /// 整体模型的Id
        /// </summary>
        public long ModelId { get; set; }

        [Column(StringLength = 80, IsNullable = false)]
        public required string ModelFileId { get; set; }
    }
}
