using System.Linq.Expressions;
using MCCS.Core.Domain.StationSites;
using MCCS.Core.Models.Devices;
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

        Task<List<ControlChannelInfo>> GetStationSiteControlChannels(long stationId, CancellationToken cancellationToken = default);

        Task<List<PseudoChannelInfo>> GetStationSitePseudoChannels(long stationId, CancellationToken cancellationToken = default);

        Task<List<DeviceInfo>> GetStationSiteDevices(long stationId, CancellationToken cancellationToken = default);

        Task<bool> AddStationSiteHardwareInfosAsync(List<StationSiteAndHardwareInfo> stationSiteHardwares, CancellationToken cancellationToken = default);

        Task<bool> DeleteStationSiteHardwareInfosAsync(long stationId, long hardwareId, CancellationToken cancellationToken = default);
    }
}
