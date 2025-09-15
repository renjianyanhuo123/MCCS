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

        Task<StationSiteAggregate> GetCurrentStationSiteAggregateAsync(CancellationToken cancellationToken = default);

        Task<bool> UpdateCurrentUseStationSiteAsync(long stationId, CancellationToken cancellationToken = default);

        Task<List<ControlChannelInfo>> GetStationSiteControlChannels(long stationId, CancellationToken cancellationToken = default);

        Task<List<PseudoChannelInfo>> GetStationSitePseudoChannels(long stationId, CancellationToken cancellationToken = default);

        Task<List<SignalInterfaceInfo>> GetStationSiteDevices(long stationId, CancellationToken cancellationToken = default);

        Task<ControlChannelInfo> GetControlChannelById(long channelId, CancellationToken cancellationToken = default);

        Task<bool> AddStationSiteHardwareInfosAsync(List<StationSiteAndHardwareInfo> stationSiteHardwares, CancellationToken cancellationToken = default);

        Task<long> AddStationSiteControlChannelAsync(ControlChannelInfo controlChannelInfo, List<ControlChannelAndSignalInfo> signals, CancellationToken cancellationToken = default);

        Task<bool> UpdateStationSiteControlChannelAsync(ControlChannelInfo controlChannelInfo, List<ControlChannelAndSignalInfo> signals, CancellationToken cancellationToken = default);

        Task<bool> DeleteStationSiteHardwareInfosAsync(long stationId, long signalId, CancellationToken cancellationToken = default);

        Task<bool> DeleteStationSiteControlChannelAsync(long channelId,CancellationToken cancellationToken = default);

        Task<List<ControlChannelAndSignalInfo>> GetControlChannelAndSignalInfosAsync(Expression<Func<ControlChannelAndSignalInfo, bool>> expression, CancellationToken cancellationToken = default);

        Task<List<ControlChannelAndModel3DInfo>> GetControlChannelAndModelInfoByModelIdAsync(long modelId,
            CancellationToken cancellationToken = default);

        List<ControlChannelInfo> GetControlChannelByModelFileId(string modelFileId);
        Task<bool> AddControlChannelAndModelInfoAsync(List<ControlChannelAndModel3DInfo> controlChannelAndModel3DInfos, CancellationToken cancellationToken = default);
    }
}
