using MCCS.Core.Domain;
using MCCS.Core.Models.SystemManager;

namespace MCCS.Core.Repositories
{
    public class ChannelAggregateRepository(IFreeSql freeSql) : IChannelAggregateRepository
    {
        public async Task<bool> AddChannelAsync(ChannelInfo channelInfo, CancellationToken cancellationToken = default)
        {
            return await freeSql.Insert(channelInfo)
                .ExecuteAffrowsAsync(cancellationToken: cancellationToken) > 0;
        }

        public async Task<VariableInfo> GetVariableInfoById(long id, CancellationToken cancellationToken = default)
        {
            return await freeSql.Select<VariableInfo>()
                .Where(a => a.Id == id)
                .FirstAsync(cancellationToken: cancellationToken);
        }

        public async Task<ChannelAggregate?> GetChannelByIdAsync(long id, CancellationToken cancellationToken = default)
        {
            var channelInfo = await freeSql.Select<ChannelInfo>()
                .Where(a => a.Id == id)
                .FirstAsync(cancellationToken: cancellationToken);
            if (channelInfo == null) return null;
            var channelVariables = await freeSql.Select<ChannelAndVariable,VariableInfo>() 
                .InnerJoin((a,b) => a.VariableId == b.Id)
                .Where((a,b) => a.ChannelId == id)
                .ToListAsync((a,b) => b, cancellationToken);
            var channelHardwares = await freeSql.Select<ChannelAndHardware, HardwareInfo>()
                .InnerJoin((a, b) => a.HardwareId == b.Id)
                .Where((a, b) => a.ChannelId == id)
                .ToListAsync((a, b) => b, cancellationToken);
            var res = new ChannelAggregate(channelInfo, channelVariables, channelHardwares);
            return res;
        }

        public async Task<List<ChannelAggregate>> GetChannelsAsync(CancellationToken cancellation = default)
        {
            var channelVariables = await freeSql.Select<ChannelAndVariable, VariableInfo>()
                .InnerJoin((a, b) => a.VariableId == b.Id) 
                .ToListAsync((a, b) => new
                {
                    a.ChannelId,
                    b
                }, cancellation);
            var channelHardwares = await freeSql.Select<ChannelAndHardware, HardwareInfo>()
                .InnerJoin((a, b) => a.HardwareId == b.Id)
                .ToListAsync((a, b) => new
                {
                    a.ChannelId,
                    b
                }, cancellation);
            var channels = await freeSql.Select<ChannelInfo>()
                .ToListAsync(cancellation);
            var res = (from channel in channels
                let variables = channelVariables.Where(a => a.ChannelId == channel.Id)
                    .Select(a => a.b)
                    .ToList()
                let hardwares = channelHardwares.Where(a => a.ChannelId == channel.Id)
                    .Select(a => a.b)
                    .ToList()
                select new ChannelAggregate(channel, variables, hardwares)).ToList();
            return res;
        } 
    }
}
