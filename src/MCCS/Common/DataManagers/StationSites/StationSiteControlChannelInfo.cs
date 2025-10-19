using Newtonsoft.Json;

namespace MCCS.Common.DataManagers.StationSites
{
    [method: JsonConstructor]
    public sealed class StationSiteControlChannelInfo(long id, string name)
    {
        public long Id { get; private set; } = id;

        public string Name { get; private set; } = name;

        public List<StationSiteControllerSignalInfo> BindSignals { get; set; }
    }
}
