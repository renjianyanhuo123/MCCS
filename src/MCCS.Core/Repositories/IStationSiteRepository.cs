using System.Linq.Expressions;
using MCCS.Core.Models.StationSites;

namespace MCCS.Core.Repositories
{
    public interface IStationSiteRepository
    {
        Task<long> AddStationInfoAsync(StationSiteInfo stationSiteInfo, CancellationToken cancellationToken = default);

        Task<List<StationSiteInfo>> GetStationSitesAsync(Expression<Func<StationSiteInfo, bool>> expression, CancellationToken cancellationToken = default);
    }
}
