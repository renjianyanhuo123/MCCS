using System.Linq.Expressions;
using MCCS.Core.Domain.StationSites;
using MCCS.Core.Models.StationSites;

namespace MCCS.Core.Repositories
{
    public interface IStationSiteAggregateRepository
    {
        Task<long> AddStationInfoAsync(StationSiteInfo stationSiteInfo, CancellationToken cancellationToken = default);

        Task<List<StationSiteInfo>> GetStationSitesAsync(Expression<Func<StationSiteInfo, bool>> expression, CancellationToken cancellationToken = default);
         
        Task<StationSiteAggregate> GetStationSiteAggregateAsync(
            long stationId,
            CancellationToken cancellationToken = default);
    }
}
