using MCCS.Core.Domain.StationSites;
using MCCS.Core.Models.StationSites;
using System.Linq.Expressions;

namespace MCCS.Core.Repositories
{
    public class StationSiteAggregateRepository(IFreeSql freeSql) : IStationSiteAggregateRepository
    {
        public async Task<long> AddStationInfoAsync(StationSiteInfo stationSiteInfo, CancellationToken cancellationToken)
        {
            var addId = await freeSql.Insert(stationSiteInfo).ExecuteIdentityAsync(cancellationToken);
            return addId;
        }

        public async Task<StationSiteAggregate> GetStationSiteAggregateAsync(long stationId, CancellationToken cancellationToken = default)
        {
            var stationSite = await freeSql.Select<StationSiteInfo>()
                .Where(s => s.Id == stationId)
                .FirstAsync(cancellationToken);
            if (stationSite == null) throw new KeyNotFoundException($"StationSiteInfo with Id {stationId} not found.");
            var pseudoChannels = await freeSql.Select<PseudoChannelInfo>()
                .Where(pc => pc.StationId == stationId)
                .ToListAsync(cancellationToken);
            var controlChannels = await freeSql.Select<ControlChannelInfo>()
                .Where(cc => cc.StationId == stationId)
                .ToListAsync(cancellationToken);
            var aggregate = new StationSiteAggregate
            {
                StationSiteInfo = stationSite,
                ControlChannelInfos = controlChannels,
                PseudoChannelInfos = pseudoChannels
            };
            return aggregate;
        } 

        public async Task<List<StationSiteInfo>> GetStationSitesAsync(Expression<Func<StationSiteInfo, bool>> expression, CancellationToken cancellationToken = default)
        {
            return await freeSql.Select<StationSiteInfo>()
                .Where(expression)
                .ToListAsync(cancellationToken);
        }
    }
}
