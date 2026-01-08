using MCCS.Infrastructure.Models.StationSites;

using Newtonsoft.Json;

namespace MCCS.Station.Abstractions.Models
{
    [method: JsonConstructor]
    public sealed class StationSiteControllerSignalInfo(
        long id,
        long belongControllerId,
        string name,
        long signalAddress,
        double upLimit,
        double downLimit,
        string unit)
    {
        public long Id { get; } = id;
        public string Name { get; } = name; 
        public long BelongControllerId { get; } = belongControllerId;
        public long SignalAddress { get; } = signalAddress;
        public double UpLimit { get; } = upLimit; 
        public double DownLimit { get; } = downLimit;
        public string Unit { get; } = unit;
        public long? LinkedDeviceId { get; set; }
    }
}
