using FreeSql.DataAnnotations;
using MCCS.Core.Devices;

namespace MCCS.Core.Models.Devices;

[Table(Name = "tb_deviceinfo")]
public class DeviceInfo : BaseModel
{
    /// <summary>
    /// 设备ID
    /// </summary>
    [Column(IsNullable = false, StringLength = 100)]
    public required string DeviceId { get; set; }
    /// <summary>
    /// 设备名称
    /// </summary>
    [Column(IsNullable = false, StringLength = 100)]
    public required string DeviceName { get; set; }
    /// <summary>
    /// 设备类型
    /// </summary>
    public DeviceTypeEnum DeviceType { get; set; }
    /// <summary>
    /// 设备描述
    /// </summary>
    [Column(IsNullable = true, StringLength = 500)]
    public string? Description { get; set; }

    /// <summary>
    /// 主设备ID
    /// </summary>
    [Column(IsNullable = true, StringLength = 100)]
    public string? MainDeviceId { get; set; }

    /// <summary>
    /// 采集频率（单位：Hz）
    /// </summary>
    public double Frequency { get; set; } = 20.0;

    /// <summary>
    /// 采集方式
    /// </summary>
    public CollectionMode Mode { get; set; } = CollectionMode.Scheduled;

    /// <summary>
    /// 连接方式
    /// </summary>
    public ConnectionTypeEnum ConnectionType { get; set; } = ConnectionTypeEnum.Mock;

    /// <summary>
    /// 设备的连接信息
    /// </summary>
    [Column(IsNullable = true, StringLength = 500)]
    public string? ConnectionInfo { get; set; }
}
