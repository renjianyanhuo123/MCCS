using MCCS.Core.Models.StationSites;
using System.Linq.Expressions;

namespace MCCS.Core.Repositories
{
    public class StationSiteRepository(IFreeSql freeSql) : IStationSiteRepository
    {
        public async Task<long> AddStationInfoAsync(StationSiteInfo stationSiteInfo, CancellationToken cancellationToken)
        {
            var addId = await freeSql.Insert(stationSiteInfo).ExecuteIdentityAsync(cancellationToken);
            return addId;
        }

        public async Task<List<StationSiteInfo>> GetStationSitesAsync(Expression<Func<StationSiteInfo, bool>> expression, CancellationToken cancellationToken = default)
        {
            return await freeSql.Select<StationSiteInfo>()
                .Where(expression)
                .ToListAsync(cancellationToken);
        }
    }
}
