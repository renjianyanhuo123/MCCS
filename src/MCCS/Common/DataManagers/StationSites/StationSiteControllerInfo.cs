using Newtonsoft.Json;

namespace MCCS.Common.DataManagers.StationSites
{
    public sealed class StationSiteControllerInfo
    {
        [JsonConstructor]
        public StationSiteControllerInfo(long id,
            string name)
        {
            Id = id;
            Name = name;
            Status = StationSiteControllerStatusEnum.DisConnected;
        }

        public long Id { get; } 
        public string Name { get; }
        public StationSiteControllerStatusEnum Status { get; }
        public List<StationSiteControllerSignalInfo> SignalInfos { get; } = []; 
    }
}
