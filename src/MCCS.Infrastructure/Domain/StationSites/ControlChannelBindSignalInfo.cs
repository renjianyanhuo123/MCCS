using MCCS.Infrastructure.Models.StationSites;

namespace MCCS.Infrastructure.Domain.StationSites
{
    public class ControlChannelBindSignalInfo
    {
        public required ControlChannelInfo ControlChannelInfo { get; set; }

        public List<ControlChannelSignal> Signals { get; set; } = [];
    }

    public class ControlChannelSignal
    {
        public required long SignalId { get; set; }
        public SignalTypeEnum SignalType { get; set; }
    }
}
