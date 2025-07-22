using FreeSql.DataAnnotations;

namespace MCCS.Core.Models.SystemManager
{
    [Table(Name = "tb_hardwareInfo")]
    public class HardwareInfo : BaseModel
    {
        [Column(IsNullable = false, StringLength = 100)]
        public required string Name { get; set; }
        /// <summary>
        /// 硬件类型
        /// </summary>
        public HardwareTypeEnum Type { get; set; }
        /// <summary>
        /// 通讯协议类型
        /// </summary>
        public CommunicationTypeEnum CommunicationType { get; set; }
        /// <summary>
        /// 连接组件Id
        /// </summary>
        [Column(IsNullable = true)]
        public long? ConnectComponentId { get; set; }

        /// <summary>
        /// 通讯连接的参数
        /// text类型
        /// Dictionary
        /// </summary>
        [Column(IsNullable = true, StringLength = -2)]
        public string? CommunicationParams { get; set; }
    }
}
