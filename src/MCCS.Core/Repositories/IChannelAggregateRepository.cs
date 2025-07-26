using MCCS.Core.Domain;
using MCCS.Core.Models.Devices;
using MCCS.Core.Models.SystemManager;

namespace MCCS.Core.Repositories
{
    public interface IChannelAggregateRepository
    {
        Task<List<ChannelAggregate>> GetChannelsAsync(CancellationToken cancellation = default);
        
        Task<ChannelAggregate?> GetChannelByIdAsync(long id, CancellationToken cancellationToken = default);

        Task<long> AddChannelAsync(ChannelInfo channelInfo, CancellationToken cancellationToken = default);

        Task<VariableInfo> GetVariableInfoByIdAsync(long id, CancellationToken cancellationToken = default);

        Task<ChannelInfo> GetChannelInfoByIdAsync(long id, CancellationToken cancellationToken = default);

        VariableInfo GetVariableInfoById(long id);

        ChannelInfo GetChannelInfoById(long id);

        List<DeviceInfo> GetHardwareInfoByChannelId(long channelId);

        List<long> GetAllChannelHardwareIds();

        Task<bool> DeleteChannelHardware(long channelId, long hardwareId, CancellationToken cancellationToken = default);

        Task<bool> AddChannelHardware(long channelId, long hardwareId, CancellationToken cancellationToken = default);
    }
}
