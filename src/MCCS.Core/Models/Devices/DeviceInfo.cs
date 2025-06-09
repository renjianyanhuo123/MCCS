using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCCS.Core.Models.Devices;

public class DeviceInfo : BaseModel
{
    /// <summary>
    /// 设备ID
    /// </summary>
    public string DeviceId { get; set; }
    /// <summary>
    /// 设备名称
    /// </summary>
    public string DeviceName { get; set; }
    /// <summary>
    /// 设备类型
    /// </summary>
    public DeviceTypeEnum DeviceType { get; set; }
    /// <summary>
    /// 设备描述
    /// </summary>
    public string Description { get; set; }
    /// <summary>
    /// 设备的连接信息
    /// </summary>
    public Dictionary<string, string> ConnectionInfo { get; set; }
}
