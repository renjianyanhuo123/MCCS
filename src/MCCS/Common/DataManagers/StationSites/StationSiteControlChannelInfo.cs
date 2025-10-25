using MCCS.Core.Models.StationSites;
using Newtonsoft.Json;

namespace MCCS.Common.DataManagers.StationSites
{
    [method: JsonConstructor]
    public sealed class StationSiteControlChannelInfo(long id, string name, ChannelTypeEnum channelType)
    {
        public long Id { get; private set; } = id;

        public string Name { get; private set; } = name;

        public ChannelTypeEnum ChannelType { get; private set; } = channelType;

        public List<StationSiteControllerSignalInfo> BindSignals { get; set; } = [];
    }
}
