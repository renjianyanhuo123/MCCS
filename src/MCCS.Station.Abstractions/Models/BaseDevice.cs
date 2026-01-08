using MCCS.Infrastructure.Models.Devices;
using MCCS.Station.Abstractions.Enums;

namespace MCCS.Station.Abstractions.Models; 

/// <summary>
/// 设备基类 - 封装通用逻辑
/// </summary>
public class BaseDevice(long id, string name, DeviceTypeEnum type)
{
    public long Id { get; } = id;

    public string Name { get; } = name; 

    public DeviceTypeEnum Type { get; private set; } = type;

    public DeviceStatusEnum Status { get; private set; } = DeviceStatusEnum.Connected;
}