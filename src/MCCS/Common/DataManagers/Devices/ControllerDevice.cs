using MCCS.Common.DataManagers.StationSites;
using MCCS.Core.Models.Devices;

namespace MCCS.Common.DataManagers.Devices
{
    public class ControllerDevice(long id, string name, long? parentDevicelong) : BaseDevice(id, name, DeviceTypeEnum.Controller, parentDevicelong)
    {

        public List<StationSiteControllerSignalInfo> SignalInfos { get; } = [];
    }
}
