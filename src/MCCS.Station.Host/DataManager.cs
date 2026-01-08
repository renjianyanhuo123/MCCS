using MCCS.Station.Abstractions.Models;

namespace MCCS.Station.Host
{
    internal class DataManager
    {
        public static bool IsMock { get; set; } = true;

        public static StationSiteInfo? StationSite { get; set; }
    }
}
