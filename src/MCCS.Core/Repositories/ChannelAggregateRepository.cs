using MCCS.Core.Domain;
using MCCS.Core.Models.Devices;
using MCCS.Core.Models.SystemManager;

namespace MCCS.Core.Repositories
{
    public class ChannelAggregateRepository(IFreeSql freeSql) : IChannelAggregateRepository
    {
        public async Task<long> AddChannelAsync(ChannelInfo channelInfo, CancellationToken cancellationToken = default)
        {
            return await freeSql.Insert(channelInfo)
                .ExecuteIdentityAsync(cancellationToken: cancellationToken);
        }

        public async Task<VariableInfo> GetVariableInfoByIdAsync(long id, CancellationToken cancellationToken = default)
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
            var channelVariables = await freeSql.Select<VariableInfo>()
                .Where(a => a.ChannelId == id)
                .ToListAsync(cancellationToken);
            var channelHardwares = await freeSql.Select<ChannelAndHardware, DeviceInfo>()
                .InnerJoin((a, b) => a.DeviceId == b.Id)
                .Where((a, b) => a.ChannelId == id)
                .ToListAsync((a, b) => b, cancellationToken);
            var res = new ChannelAggregate(channelInfo, channelVariables, channelHardwares);
            return res;
        }

        public async Task<List<ChannelAggregate>> GetChannelsAsync(CancellationToken cancellation = default)
        {
            var channelVariables = await freeSql.Select<VariableInfo>() 
                .ToListAsync(cancellation);
            var channelHardwares = await freeSql.Select<ChannelAndHardware, DeviceInfo>()
                .InnerJoin((a, b) => a.DeviceId == b.Id)
                .ToListAsync((a, b) => new
                {
                    a.ChannelId,
                    b
                }, cancellation);
            var channels = await freeSql.Select<ChannelInfo>()
                .ToListAsync(cancellation);
            var res = (from channel in channels
                let variables = channelVariables.Where(a => a.ChannelId == channel.Id)
                    .ToList()
                let hardwares = channelHardwares.Where(a => a.ChannelId == channel.Id)
                    .Select(a => a.b)
                    .ToList()
                select new ChannelAggregate(channel, variables, hardwares)).ToList();
            return res;
        }
        
        public async Task<ChannelInfo> GetChannelInfoByIdAsync(long id, CancellationToken cancellationToken = default)
        {
            return await freeSql.Select<ChannelInfo>()
                .Where(c => c.Id == id)
                .FirstAsync(cancellationToken);
        }

        public ChannelAggregate? GetChannelById(long id)
        {
            var channelInfo = freeSql.Select<ChannelInfo>()
                .Where(a => a.Id == id)
                .First();
            if (channelInfo == null) return null;
            var channelVariables = freeSql.Select<VariableInfo>() 
                .Where(a => a.ChannelId == id)
                .ToList();
            var channelHardwares = freeSql.Select<ChannelAndHardware, DeviceInfo>()
                .InnerJoin((a, b) => a.DeviceId == b.Id)
                .Where((a, b) => a.ChannelId == id)
                .ToList((a, b) => b);
            var res = new ChannelAggregate(channelInfo, channelVariables, channelHardwares);
            return res;
        }

        public VariableInfo GetVariableInfoById(long id)
        {
            return freeSql.Select<VariableInfo>()
                .Where(c => c.Id == id)
                .ToOne();
        }

        public ChannelInfo GetChannelInfoById(long id)
        {
            return freeSql.Select<ChannelInfo>()
                .Where(c => c.Id == id)
                .ToOne();
        }

        public List<DeviceInfo> GetHardwareInfoByChannelId(long channelId)
        {
            return freeSql.Select<ChannelAndHardware, DeviceInfo>()
                .InnerJoin((a, b) => a.DeviceId == b.Id)
                .Where((a, b) => a.ChannelId == channelId) 
                .ToList((a, b) => b);
        }

        public List<long> GetAllChannelHardwareIds()
        {
            return freeSql.Select<ChannelAndHardware>()
                .Where(c => true)
                .ToList(s => s.DeviceId);
        }

        public async Task<bool> DeleteChannelHardware(long channelId, long hardwareId, CancellationToken cancellationToken = default)
        {
            return await freeSql.Delete<ChannelAndHardware>()
                .Where(c => c.ChannelId == channelId && c.DeviceId == hardwareId)
                .ExecuteAffrowsAsync(cancellationToken) > 0;
        }

        public async Task<bool> AddChannelHardware(long channelId, long hardwareId, CancellationToken cancellationToken = default)
        {
            return await freeSql.Insert(new ChannelAndHardware
            {
                ChannelId = channelId,
                DeviceId = hardwareId
            }).ExecuteAffrowsAsync(cancellationToken) > 0;
        }

        public async Task<bool> UpdateChannelInfoAsync(long channelId, string channelName, bool isShowable, bool isOpenSpecimenProtected, CancellationToken cancellationToken = default)
        {
            return await freeSql.Update<ChannelInfo>(channelId)
                .Set(a => a.ChannelName, channelName)
                .Set(a => a.IsShowable, isShowable)
                .Set(a => a.IsOpenSpecimenProtected, isOpenSpecimenProtected)
                .ExecuteAffrowsAsync(cancellationToken) > 0;
        }

        public async Task<bool> UpdateVariableInfoAsync(VariableInfo variableInfo, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(variableInfo.HardwareInfos)) variableInfo.HardwareInfos = null;
            return await freeSql.Update<VariableInfo>(variableInfo.Id)
                .Set(a => a.Name, variableInfo.Name)
                .Set(a => a.IsShowable, variableInfo.IsShowable)
                .Set(a => a.IsCanControl, variableInfo.IsCanControl)
                .Set(a => a.IsCanSetLimit, variableInfo.IsCanSetLimit)
                .Set(a => a.IsCanCalibration, variableInfo.IsCanCalibration)
                .Set(a => a.HardwareInfos, variableInfo.HardwareInfos)
                .ExecuteAffrowsAsync(cancellationToken) > 0;
        }
    }
}
