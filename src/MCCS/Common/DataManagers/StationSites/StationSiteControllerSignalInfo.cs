using Newtonsoft.Json;

namespace MCCS.Common.DataManagers.StationSites
{
    public sealed class StationSiteControllerSignalInfo
    {
        [JsonConstructor]
        public StationSiteControllerSignalInfo(long id,
            string name)
        {
            Id = id;
            Name = name;
        }

        public long Id { get; }
        public string Name { get; }

        public StationSiteDeviceInfo? LinkedDevice { get; private set; }

        public void Link(StationSiteDeviceInfo device)
        {
            LinkedDevice = device;
        }
    }
}
