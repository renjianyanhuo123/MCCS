using FreeSql.DataAnnotations;

namespace MCCS.Core.Models.SystemManager
{
    /// <summary>
    /// 通道和变量中间表
    /// </summary>
    [Table(Name = "tb_channelAndVariable")]
    public class ChannelAndVariable : BaseModel
    {
        public long ChannelId { get; set; }
        public long VariableId { get; set; }
    }
}
