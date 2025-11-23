using MCCS.Infrastructure.Models.Devices;
using MCCS.Infrastructure.Models.StationSites;

namespace MCCS.Infrastructure.Domain.StationSites
{
    public class ControlChannelBindSignalInfo
    {
        public ControlChannelInfo ControlChannelInfo { get; set; }

        public List<ControlChannelSignal> Signals { get; set; }
    }

    public class ControlChannelSignal
    {
        public SignalInterfaceInfo SignalInfo { get; set; }
        public SignalTypeEnum SignalType { get; set; }
        public DeviceInfo? LinkDeviceInfo { get; set; } = null;
    }
}
