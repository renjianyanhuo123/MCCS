using MCCS.Infrastructure.Models.Devices;

using Newtonsoft.Json;

namespace MCCS.Station.Abstractions.Models
{
    [method: JsonConstructor]
    public class ControllerDevice(long id, string name) : BaseDevice(id, name, DeviceTypeEnum.Controller)
    {
        public List<long> SignalIds { get; } = [];
    }
}
