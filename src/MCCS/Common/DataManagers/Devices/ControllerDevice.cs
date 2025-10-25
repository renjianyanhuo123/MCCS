using MCCS.Common.DataManagers.StationSites;
using MCCS.Core.Models.Devices;

namespace MCCS.Common.DataManagers.Devices
{
    public class ControllerDevice(long id, string name) : BaseDevice(id, name, DeviceTypeEnum.Controller)
    {
        public List<StationSiteControllerSignalInfo> SignalInfos { get; } = [];
    }
}
