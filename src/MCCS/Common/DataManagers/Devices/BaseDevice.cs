using MCCS.Infrastructure.Models.Devices;
using MCCS.Station.Core.Devices; 

namespace MCCS.Common.DataManagers.Devices;

/// <summary>
/// 设备基类 - 封装通用逻辑
/// </summary>
public class BaseDevice(long id, string name, DeviceTypeEnum type, long? parentDeviceId)
{
    public long Id { get; } = id;

    public string Name { get; } = name; 
    public long? ParentDeviceId { get; set; } = parentDeviceId;

    public DeviceTypeEnum Type { get; private set; } = type;

    public DeviceStatusEnum Status { get; private set; } = DeviceStatusEnum.Connected;
}