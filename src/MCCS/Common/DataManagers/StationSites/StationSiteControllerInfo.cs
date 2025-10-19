using Newtonsoft.Json;

namespace MCCS.Common.DataManagers.StationSites
{
    [method: JsonConstructor]
    public sealed class StationSiteControllerInfo(
        long id,
        string name)
    {
        public long Id { get; } = id;
        public string Name { get; } = name;
        public StationSiteControllerStatusEnum Status { get; } = StationSiteControllerStatusEnum.DisConnected;
        public List<StationSiteControllerSignalInfo> SignalInfos { get; } = []; 
    }
}
