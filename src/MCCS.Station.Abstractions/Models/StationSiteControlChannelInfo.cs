using MCCS.Infrastructure.Models.StationSites;

using Newtonsoft.Json;

namespace MCCS.Station.Abstractions.Models
{
    public class StationSiteControlChannelSignalInfo
    {
        public long SignalId { get; set; }

        public SignalTypeEnum SignalType { get; set; }
    }

    [method: JsonConstructor]
    public sealed class StationSiteControlChannelInfo(long id, string name, ChannelTypeEnum channelType, long controllerId)
    {
        public long Id { get; private set; } = id;

        public string Name { get; private set; } = name;

        public ChannelTypeEnum ChannelType { get; private set; } = channelType;

        public long ControllerId { get; private set; } = controllerId;

        public required List<StationSiteControlChannelSignalInfo> BindSignalIds { get; set; }
    }
}
