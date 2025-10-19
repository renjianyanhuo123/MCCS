using FreeSql.DataAnnotations;

namespace MCCS.Core.Models.Devices
{
    [Table(Name = "tb_signalInterfaceInfo")]
    public class SignalInterfaceInfo : BaseModel
    {
        /// <summary>
        /// 信号名称
        /// </summary>
        public string SignalName { get; set; } = string.Empty;
        /// <summary>
        /// 连接的设备ID
        /// </summary>
        public long ConnectedDeviceId { get; set; }
        /// <summary>
        /// 所属控制器设备ID
        /// </summary>
        public long BelongToControllerId { get; set; }
        /// <summary>
        /// 量程上限
        /// </summary>
        public double UpLimitRange { get; set; }
        /// <summary>
        /// 量程下限
        /// </summary>
        public double DownLimitRange { get; set; }
        /// <summary>
        /// 信号接口地址
        /// </summary>
        [Column(IsNullable = false, StringLength = 50), Obsolete]
        public string Address { get; set; } = string.Empty;
        /// <summary>
        /// 信号地址, 是采集信号的数据的来源
        /// </summary>
        public long SignalAddress { get; set; }

        /// <summary>
        /// 更新周期(单位毫秒)
        /// </summary>
        public double UpdateCycle { get; set; }
        /// <summary>
        /// 数据类型
        /// </summary>
        public SignalDataTypeEnum DataType { get; set; }
        /// <summary>
        /// 信号角色
        /// </summary>
        public SignalRoleTypeEnum SignalRole { get; set; }
        /// <summary>
        /// 权重系数
        /// </summary>
        public double WeightCoefficient { get; set; }
    }
}
