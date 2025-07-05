using MCCS.Core.Models.Devices;

namespace MCCS.Core.Devices
{
    public interface IDeviceFactory
    {
        IDevice CreateDevice(DeviceInfo deviceInfo);
    }
}
