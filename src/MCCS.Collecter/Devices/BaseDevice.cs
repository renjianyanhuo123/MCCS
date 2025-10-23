namespace MCCS.Collecter.Devices;

/// <summary>
/// 设备基类 - 封装通用逻辑
/// </summary>
public abstract class BaseDevice
{
    public string Id { get; }

    public string ConnectionId { get; }

    public bool IsActive { get; private set; }

    public string Name { get; }
    //public DeviceTypeEnum Type { get; }

    public DeviceStatusEnum Status { get; } 
}