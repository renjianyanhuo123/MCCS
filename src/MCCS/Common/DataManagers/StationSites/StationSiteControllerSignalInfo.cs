using Newtonsoft.Json;

namespace MCCS.Common.DataManagers.StationSites
{
    [method: JsonConstructor]
    public sealed class StationSiteControllerSignalInfo(
        long id,
        string name)
    {
        public long Id { get; } = id;
        public string Name { get; } = name;

        public StationSiteDeviceInfo? LinkedDevice { get; private set; }

        public void Link(StationSiteDeviceInfo device)
        {
            LinkedDevice = device;
        }
    }
}
