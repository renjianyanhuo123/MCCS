using MCCS.Station.Abstractions.Models;

namespace MCCS.Services.StationServices
{
    public interface IStationService
    {
        Task<StationSiteInfo> GetCurrentStationSiteInfoAsync();
    }
}
