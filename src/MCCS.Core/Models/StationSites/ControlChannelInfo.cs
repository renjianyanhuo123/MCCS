using FreeSql.DataAnnotations;

namespace MCCS.Core.Models.StationSites
{
    /// <summary>
    /// 控制通道
    /// </summary>
    [Table(Name = "tb_controlChannelInfo")]
    public class ControlChannelInfo : BaseModel
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
        /// 通道类型
        /// </summary>
        public ChannelTypeEnum ChannelType { get; set; }
        /// <summary>
        /// 控制模式
        /// </summary>
        public ControlChannelModeTypeEnum ControlMode { get; set; }
        /// <summary>
        /// 是否可见
        /// </summary>
        public bool IsShowable { get; set; }
        /// <summary>
        /// 是否开启试样保护 
        /// </summary>
        public bool IsOpenSpecimenProtected { get; set; }
        /// <summary>
        /// 控制周期,单位ms
        /// </summary>
        public double ControlCycle { get; set; }
        /// <summary>
        /// 输出限制，百分比，取值范围0~100(%)
        /// </summary>
        public short OutputLimitation { get; set; }
    }
}
