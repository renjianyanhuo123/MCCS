using Newtonsoft.Json;

namespace MCCS.Common.DataManagers.StationSites
{
    public sealed class StationSiteDeviceInfo
    {
        [JsonConstructor]
        public StationSiteDeviceInfo(long id, string name)
        {
            Id = id;
            Name = name;
        }

        public long Id { get; }

        public string Name { get; }
    }
}
