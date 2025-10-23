using FreeSql.DataAnnotations;
using MCCS.WorkflowSetting.Models.Edges;

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
    /// 采集频率（单位：Hz）
    /// </summary>
    public double Frequency { get; set; } = 20.0;  
    /// <summary>
    /// 功能类型
    /// </summary>
    public FunctionTypeEnum FunctionType { get; set; }

    /// <summary>
    /// 设备的连接信息
    /// </summary>
    [Column(IsNullable = true, StringLength = -2)]
    public string? ConnectionInfo { get; set; }
}
