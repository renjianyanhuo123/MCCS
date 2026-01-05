using MCCS.Infrastructure.Models.Devices;

namespace MCCS.Station.Abstractions.Models
{
    public class ControllerDevice(long id, string name, long? parentDevicelong) : BaseDevice(id, name, DeviceTypeEnum.Controller, parentDevicelong)
    {
        public List<StationSiteControllerSignalInfo> SignalInfos { get; } = [];
    }
}
