using FreeSql.DataAnnotations;

namespace MCCS.Core.Models.SystemManager
{
    /// <summary>
    /// 通道和硬件中间表
    /// </summary>
    [Table(Name = "tb_channelAndHardware")]
    public class ChannelAndHardware : BaseModel
    {
        public long ChannelId { get; set; }
        public long HardwareId { get; set; }
    }
}
