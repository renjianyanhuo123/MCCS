using MCCS.Core.Domain;
using MCCS.Core.Models.SystemManager;

namespace MCCS.Core.Repositories
{
    public interface IChannelAggregateRepository
    {
        Task<List<ChannelAggregate>> GetChannelsAsync(CancellationToken cancellation = default);
        
        Task<ChannelAggregate?> GetChannelByIdAsync(long id, CancellationToken cancellationToken = default);

        Task<bool> AddChannelAsync(ChannelInfo channelInfo, CancellationToken cancellationToken = default);

        Task<VariableInfo> GetVariableInfoById(long id, CancellationToken cancellationToken = default);
    }
}
