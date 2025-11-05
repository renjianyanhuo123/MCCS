using FreeSql.DataAnnotations;

namespace MCCS.Core.Models.StationSites
{
    /// <summary>
    /// 虚拟通道
    /// </summary>
    [Table(Name = "stationSite_PseudoChannelInfo")]
    public class PseudoChannelInfo : BaseModel
    {
        public long StationId { get; set; }
        /// <summary>
        /// 内部名称
        /// </summary>
        [Column(IsNullable = false, StringLength = 50)]
        public required string ChannelId { get; set; } = string.Empty;

        [Column(IsNullable = false, StringLength = 100)]
        public required string ChannelName { get; set; }
        /// <summary>
        /// 范围最小值
        /// </summary>
        public double RangeMin { get; set; }
        /// <summary>
        /// 范围最大值
        /// </summary>
        public double RangeMax { get; set; }
        /// <summary>
        /// 计算公式
        /// </summary>
        [Column(IsNullable = false, StringLength = -2)]
        public string Formula { get; set; } = string.Empty;
        /// <summary>
        /// 单位;None---默认无单位
        /// </summary>
        [Column(IsNullable = true, StringLength = 50)]
        public string? Unit { get; set; } = null;

        /// <summary>
        /// 是否可校准
        /// </summary>
        public bool HasTare { get; set; }
    }
}
