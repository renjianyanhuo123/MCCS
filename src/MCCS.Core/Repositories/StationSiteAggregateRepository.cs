using MCCS.Core.Domain.StationSites;
using MCCS.Core.Models.Devices;
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

        public async Task<bool> AddStationSiteHardwareInfosAsync(List<StationSiteAndHardwareInfo> stationSiteHardwares, CancellationToken cancellationToken = default)
        {
            return await freeSql.Insert(stationSiteHardwares).ExecuteAffrowsAsync(cancellationToken) > 0;
        }

        public async Task<bool> DeleteStationSiteHardwareInfosAsync(long stationId, long hardwareId, CancellationToken cancellationToken)
        {
            return await freeSql.Delete<StationSiteAndHardwareInfo>()
                .Where(s => s.StationId == stationId && s.HardwareId == hardwareId)
                .ExecuteAffrowsAsync(cancellationToken) > 0;
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
            var devices = await freeSql.Select<StationSiteAndHardwareInfo, DeviceInfo>()
                .LeftJoin((a, b) => a.HardwareId == b.Id)
                .Where((a, b) => a.StationId == stationId)
                .ToListAsync((a, b) => b, cancellationToken);
            var aggregate = new StationSiteAggregate
            {
                StationSiteInfo = stationSite,
                ControlChannelInfos = controlChannels,
                PseudoChannelInfos = pseudoChannels,
                DeviceInfos = devices
            };
            return aggregate;
        }

        public async Task<List<ControlChannelInfo>> GetStationSiteControlChannels(long stationId, CancellationToken cancellationToken = default)
        {
            return await freeSql.Select<ControlChannelInfo>()
                .Where(cc => cc.StationId == stationId)
                .ToListAsync(cancellationToken);
        }

        public Task<List<DeviceInfo>> GetStationSiteDevices(long stationId, CancellationToken cancellationToken = default)
        {
            return freeSql.Select<StationSiteAndHardwareInfo, DeviceInfo>()
                .LeftJoin((a,b) => a.HardwareId == b.Id)
                .Where((a,b) => a.StationId == stationId)
                .ToListAsync((a,b) => b, cancellationToken);
        }

        public Task<List<PseudoChannelInfo>> GetStationSitePseudoChannels(long stationId, CancellationToken cancellationToken = default)
        {
            return freeSql.Select<PseudoChannelInfo>()
                .Where(pc => pc.StationId == stationId)
                .ToListAsync(cancellationToken);
        }

        public async Task<List<StationSiteInfo>> GetStationSitesAsync(Expression<Func<StationSiteInfo, bool>> expression, CancellationToken cancellationToken = default)
        {
            return await freeSql.Select<StationSiteInfo>()
                .Where(expression)
                .ToListAsync(cancellationToken);
        }
    }
}
