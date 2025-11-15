using MCCS.Core.Domain.StationSites;
using MCCS.Core.Models.Devices;
using MCCS.Core.Models.StationSites;
using System.Linq.Expressions;
using MCCS.Core.Domain;
using MCCS.Core.Models.Model3D;

namespace MCCS.Core.Repositories
{
    public class StationSiteAggregateRepository(IFreeSql freeSql) : IStationSiteAggregateRepository
    {
        public async Task<bool> AddControlChannelAndModelInfoAsync(List<ControlChannelAndModel3DInfo> controlChannelAndModel3DInfos, CancellationToken cancellationToken)
        {
            return await freeSql.Insert(controlChannelAndModel3DInfos).ExecuteAffrowsAsync(cancellationToken) > 0;
        }

        public async Task<long> AddStationInfoAsync(StationSiteInfo stationSiteInfo, CancellationToken cancellationToken)
        {
            var addId = await freeSql.Insert(stationSiteInfo).ExecuteIdentityAsync(cancellationToken);
            return addId;
        }

        public async Task<long> AddStationSiteControlChannelAsync(ControlChannelInfo controlChannelInfo, List<ControlChannelAndSignalInfo> signals, CancellationToken cancellationToken = default)
        {
            using var uow = freeSql.CreateUnitOfWork();
            var addId = await uow.Orm.Insert(controlChannelInfo).ExecuteIdentityAsync(cancellationToken);
            foreach (var signal in signals)
            {
                signal.ChannelId = addId;
            }
            await uow.Orm.Insert(signals).ExecuteAffrowsAsync(cancellationToken);
            uow.Commit();
            return addId;
        }

        public async Task<bool> AddStationSiteHardwareInfosAsync(List<StationSiteAndHardwareInfo> stationSiteHardwares, CancellationToken cancellationToken = default)
        {
            return await freeSql.Insert(stationSiteHardwares).ExecuteAffrowsAsync(cancellationToken) > 0;
        }

        public async Task<bool> DeleteStationSiteControlChannelAsync(long channelId, CancellationToken cancellationToken = default)
        {
            using var uow = freeSql.CreateUnitOfWork();
            var count1 = await uow.Orm.Update<ControlChannelInfo>()
                .Set(s => s.IsDeleted, true)
                .Where(s => s.Id == channelId)
                .ExecuteAffrowsAsync(cancellationToken);
            var count2 = await uow.Orm.Delete<ControlChannelAndSignalInfo>().Where(s => s.ChannelId == channelId)
                .ExecuteAffrowsAsync(cancellationToken);
            uow.Commit();
            return count1 >= 0 && count2 >= 0;

        }

        public async Task<bool> DeleteStationSiteHardwareInfosAsync(long stationId, long signalId, CancellationToken cancellationToken)
        {
            return await freeSql.Delete<StationSiteAndHardwareInfo>()
                .Where(s => s.StationId == stationId && s.SignalId == signalId)
                .ExecuteAffrowsAsync(cancellationToken) > 0;
        }

        public async Task<List<ControlChannelAndSignalInfo>> GetControlChannelAndSignalInfosAsync(Expression<Func<ControlChannelAndSignalInfo, bool>> expression, CancellationToken cancellationToken = default)
        {
            return await freeSql.Select<ControlChannelAndSignalInfo>()
                .Where(expression)
                .ToListAsync(cancellationToken);
        }

        public async Task<ControlChannelInfo> GetControlChannelById(long channelId, CancellationToken cancellationToken = default)
        {
            return await freeSql.Select<ControlChannelInfo>()
                .Where(c => c.Id == channelId)
                .FirstAsync(cancellationToken);
        }

        public List<ControlChannelInfo> GetControlChannelByModelFileId(string modelFileId)
        {
            return freeSql.Select<ControlChannelAndModel3DInfo, ControlChannelInfo>()
                .LeftJoin((a, b) => a.ControlChannelId == a.Id)
                .Where((a, b) => a.ModelFileId == modelFileId)
                .ToList((a, b) => b);
        }

        public async Task<List<ControlChannelAndModel3DInfo>> GetControlChannelAndModelInfoByModelIdAsync(long modelId,
            CancellationToken cancellationToken)
        {
            return await freeSql.Select<ControlChannelAndModel3DInfo>()
                .Where(a => a.ModelId == modelId)
                .ToListAsync(cancellationToken);
        }

        public async Task<StationSiteAggregate> GetStationSiteAggregateAsync(long stationId, CancellationToken cancellationToken = default)
        {
            var stationSite = await freeSql.Select<StationSiteInfo>()
                .Where(s => s.Id == stationId && s.IsDeleted ==false)
                .FirstAsync(cancellationToken);
            if (stationSite == null) throw new KeyNotFoundException($"StationSiteInfo with Id {stationId} not found.");
            var pseudoChannels = await freeSql.Select<PseudoChannelInfo>()
                .Where(pc => pc.StationId == stationId && pc.IsDeleted == false)
                .ToListAsync(cancellationToken);
            var controlChannels = await freeSql.Select<ControlChannelInfo>()
                .Where(cc => cc.StationId == stationId && cc.IsDeleted == false)
                .ToListAsync(cancellationToken);
            var controlChannelIds = controlChannels.Select(s => s.Id).ToList();
            var controlChannelSignals = await freeSql
                .Select<ControlChannelAndSignalInfo, ControlChannelInfo, SignalInterfaceInfo, DeviceInfo>()
                .LeftJoin((a,b,c,d) => a.ChannelId == b.Id)
                .LeftJoin((a, b, c,d) => a.SignalId == c.Id)
                .LeftJoin((a, b, c, d) => a.DeviceId == d.Id)
                .Where((a, b, c, d) => controlChannelIds.Contains(a.ChannelId))
                .ToListAsync((a, b, c, d) => new 
                {
                    ChannelId = b.Id,
                    ControlChannelAndSignalInfo = a, 
                    SignalInterfaceInfo = c,
                    LinkDeviceInfo = d
                }, cancellationToken);
            var pseudoChannelIds = pseudoChannels.Select(s => s.Id).ToList();
            var pseudoChannelSignals = await freeSql
                .Select<PseudoChannelAndSignalInfo, PseudoChannelInfo, SignalInterfaceInfo>()
                .LeftJoin((a, b, c) => a.PseudoChannelId == b.Id)
                .LeftJoin((a, b, c) => a.SignalId == c.Id)
                .Where((a, b, c) => pseudoChannelIds.Contains(a.PseudoChannelId))
                .ToListAsync((a, b, c) => new
                {
                    ChannelId = b.Id,
                    PseudoChannelAndSignalInfo = a,
                    SignalInterfaceInfo = c
                }, cancellationToken);
            var controlChannelAggregate = new List<ControlChannelBindSignalInfo>();
            foreach (var controlChannel in controlChannels)
            {
                var item = new ControlChannelBindSignalInfo
                {
                    ControlChannelInfo = controlChannel
                };
                var tempSignals = controlChannelSignals.Where(s => s.ChannelId == controlChannel.Id)
                    .Select(s => new ControlChannelSignal
                    {
                        SignalType = s.ControlChannelAndSignalInfo.SignalType,
                        SignalInfo = s.SignalInterfaceInfo,
                        LinkDeviceInfo = s.LinkDeviceInfo,
                    })
                    .ToList();
                item.Signals = tempSignals;
                controlChannelAggregate.Add(item);
            }
            // 虚拟通道
            var pseudoChannelAggregate = new List<PseudoChannelBindSignalInfo>();
            foreach (var pseudoChannel in pseudoChannels)
            {
                var item = new PseudoChannelBindSignalInfo
                {
                    PseudoChannelInfo = pseudoChannel
                };
                var tempSignals = pseudoChannelSignals.Where(s => s.ChannelId == pseudoChannel.Id)
                    .Select(s => s.SignalInterfaceInfo)
                    .ToList();
                item.Signals = tempSignals;
                pseudoChannelAggregate.Add(item);
            }
            var signals = await freeSql.Select<StationSiteAndHardwareInfo, SignalInterfaceInfo>()
                .LeftJoin((a, b) => a.SignalId == b.Id)
                .Where((a, b) => a.StationId == stationId)
                .ToListAsync((a, b) => b, cancellationToken);
            var modelAggregate = new Model3DAggregate();
            var modelBaseInfo = await freeSql.Select<Model3DBaseInfo>()
                .Where(m => m.StationId == stationId && m.IsDeleted == false)
                .FirstAsync(cancellationToken);
            modelAggregate.BaseInfo = modelBaseInfo;
            if (modelBaseInfo != null)
            {
                modelAggregate.Model3DDataList = await freeSql.Select<Model3DData>()
                    .Where(c => c.GroupKey == modelBaseInfo.Id)
                    .ToListAsync(cancellationToken);
            }
            var aggregate = new StationSiteAggregate
            {
                StationSiteInfo = stationSite,
                ControlChannelSignalInfos = controlChannelAggregate,
                PseudoChannelInfos = pseudoChannelAggregate,
                Signals = signals,
                Model3DAggregate = modelAggregate
            };
            return aggregate;
        }

        public async Task<List<ControlChannelInfo>> GetStationSiteControlChannels(long stationId, CancellationToken cancellationToken = default)
        {
            return await freeSql.Select<ControlChannelInfo>()
                .Where(cc => cc.StationId == stationId && cc.IsDeleted == false)
                .ToListAsync(cancellationToken);
        }

        public Task<List<SignalInterfaceInfo>> GetStationSiteDevices(long stationId, CancellationToken cancellationToken = default)
        {
            return freeSql.Select<StationSiteAndHardwareInfo, SignalInterfaceInfo>()
                .LeftJoin((a,b) => a.SignalId == b.Id)
                .Where((a,b) => a.StationId == stationId)
                .ToListAsync((a,b) => b, cancellationToken);
        }

        public Task<List<PseudoChannelInfo>> GetStationSitePseudoChannels(long stationId, CancellationToken cancellationToken = default)
        {
            return freeSql.Select<PseudoChannelInfo>()
                .Where(pc => pc.StationId == stationId && pc.IsDeleted == false)
                .ToListAsync(cancellationToken);
        }

        public async Task<PseudoChannelInfo> GetPseudoChannelById(long channelId, CancellationToken cancellationToken = default)
        {
            return await freeSql.Select<PseudoChannelInfo>()
                .Where(c => c.Id == channelId)
                .FirstAsync(cancellationToken);
        }

        public async Task<long> AddStationSitePseudoChannelAsync(PseudoChannelInfo pseudoChannelInfo, List<PseudoChannelAndSignalInfo> signals, CancellationToken cancellationToken = default)
        {
            using var uow = freeSql.CreateUnitOfWork();
            var addId = await uow.Orm.Insert(pseudoChannelInfo).ExecuteIdentityAsync(cancellationToken);
            foreach (var signal in signals)
            {
                signal.PseudoChannelId = addId;
            }
            if (signals.Count > 0)
            {
                await uow.Orm.Insert(signals).ExecuteAffrowsAsync(cancellationToken);
            }
            uow.Commit();
            return addId;
        }

        public async Task<bool> UpdateStationSitePseudoChannelAsync(PseudoChannelInfo pseudoChannelInfo, List<PseudoChannelAndSignalInfo> signals, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(pseudoChannelInfo);
            using var uow = freeSql.CreateUnitOfWork();
            var count1 = await uow.Orm.Update<PseudoChannelInfo>()
                .Set(s => s.ChannelName, pseudoChannelInfo.ChannelName)
                .Set(s => s.RangeMin, pseudoChannelInfo.RangeMin)
                .Set(s => s.RangeMax, pseudoChannelInfo.RangeMax)
                .Set(s => s.Formula, pseudoChannelInfo.Formula)
                .Set(s => s.HasTare, pseudoChannelInfo.HasTare)
                .Set(s => s.Unit, pseudoChannelInfo.Unit)
                .Where(s => s.Id == pseudoChannelInfo.Id)
                .ExecuteAffrowsAsync(cancellationToken);
            var count2 = await uow.Orm.Delete<PseudoChannelAndSignalInfo>().Where(s => s.PseudoChannelId == pseudoChannelInfo.Id)
                .ExecuteAffrowsAsync(cancellationToken);
            var count3 = 0;
            if (signals.Count > 0)
            {
                count3 = await uow.Orm.Insert(signals).ExecuteAffrowsAsync(cancellationToken);
            }
            uow.Commit();
            return count1 >= 0 && count2 >= 0 && count3 >= 0;
        }

        public async Task<bool> DeleteStationSitePseudoChannelAsync(long channelId, CancellationToken cancellationToken = default)
        {
            using var uow = freeSql.CreateUnitOfWork();
            var count1 = await uow.Orm.Update<PseudoChannelInfo>()
                .Set(s => s.IsDeleted, true)
                .Where(s => s.Id == channelId)
                .ExecuteAffrowsAsync(cancellationToken);
            var count2 = await uow.Orm.Delete<PseudoChannelAndSignalInfo>().Where(s => s.PseudoChannelId == channelId)
                .ExecuteAffrowsAsync(cancellationToken);
            uow.Commit();
            return count1 >= 0 && count2 >= 0;
        }

        public async Task<List<PseudoChannelAndSignalInfo>> GetPseudoChannelAndSignalInfosAsync(Expression<Func<PseudoChannelAndSignalInfo, bool>> expression, CancellationToken cancellationToken = default)
        {
            return await freeSql.Select<PseudoChannelAndSignalInfo>()
                .Where(expression)
                .ToListAsync(cancellationToken);
        }

        public async Task<List<StationSiteInfo>> GetStationSitesAsync(Expression<Func<StationSiteInfo, bool>> expression, CancellationToken cancellationToken = default)
        {
            return await freeSql.Select<StationSiteInfo>()
                .Where(expression)
                .ToListAsync(cancellationToken);
        }

        public async Task<bool> UpdateStationSiteControlChannelAsync(ControlChannelInfo controlChannelInfo, List<ControlChannelAndSignalInfo> signals, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(controlChannelInfo);
            using var uow = freeSql.CreateUnitOfWork();
            var count1 = await uow.Orm.Update<ControlChannelInfo>()
            .Set(s => s.ChannelName, controlChannelInfo.ChannelName)
            .Set(s => s.OutputLimitation, controlChannelInfo.OutputLimitation)
            .Set(s => s.ControlCycle, controlChannelInfo.ControlCycle)
            .Set(s => s.ControlMode, controlChannelInfo.ControlMode)
            .Set(s => s.IsShowable, controlChannelInfo.IsShowable)
            .Set(s => s.ChannelType, controlChannelInfo.ChannelType)
            .Set(s => s.IsOpenSpecimenProtected, controlChannelInfo.IsOpenSpecimenProtected)
            .Where(s => s.Id == controlChannelInfo.Id)
            .ExecuteAffrowsAsync(cancellationToken);
            var count2 = await uow.Orm.Delete<ControlChannelAndSignalInfo>().Where(s => s.ChannelId == controlChannelInfo.Id)
                .ExecuteAffrowsAsync(cancellationToken);
            var count3 = 0;
            if (signals.Count > 0)
            {
                count3 = await uow.Orm.Insert(signals).ExecuteAffrowsAsync(cancellationToken);
            }
            uow.Commit();
            return count1 >= 0 && count2 >= 0 && count3 >= 0;
        }

        public async Task<bool> UpdateCurrentUseStationSiteAsync(long stationId, CancellationToken cancellationToken = default)
        {
            using var uow = freeSql.CreateUnitOfWork();
            await uow.Orm.Update<StationSiteInfo>()
                .Set(c => c.IsUsing, false)
                .Where(c => c.IsUsing)
                .ExecuteAffrowsAsync(cancellationToken);
            await uow.Orm.Update<StationSiteInfo>()
                .Set(c => c.IsUsing, true)
                .Where(c => c.Id == stationId)
                .ExecuteAffrowsAsync(cancellationToken);
            uow.Commit();
            return true;
        }

        public async Task<StationSiteAggregate> GetCurrentStationSiteAggregateAsync(CancellationToken cancellationToken = default)
        {
            var stationSite = await freeSql.Select<StationSiteInfo>()
                .Where(s => s.IsUsing && s.IsDeleted == false)
                .FirstAsync(cancellationToken);
            if (stationSite == null) throw new KeyNotFoundException($"StationSiteInfo with Id {stationSite} not found.");
            var pseudoChannels = await freeSql.Select<PseudoChannelInfo>()
                .Where(pc => pc.StationId == stationSite.Id && pc.IsDeleted == false)
                .ToListAsync(cancellationToken);
            var controlChannels = await freeSql.Select<ControlChannelInfo>()
                .Where(cc => cc.StationId == stationSite.Id && cc.IsDeleted == false)
                .ToListAsync(cancellationToken);
            var signals = await freeSql.Select<StationSiteAndHardwareInfo, SignalInterfaceInfo>()
                .LeftJoin((a, b) => a.SignalId == b.Id)
                .Where((a, b) => a.StationId == stationSite.Id)
                .ToListAsync((a, b) => b, cancellationToken);
            var modelAggregate = new Model3DAggregate();
            var modelBaseInfo = await freeSql.Select<Model3DBaseInfo>()
                .Where(m => m.StationId == stationSite.Id && m.IsDeleted == false)
                .FirstAsync(cancellationToken);
            var controlChannelIds = controlChannels.Select(s => s.Id).ToList();
            var controlChannelSignals = await freeSql
                .Select<ControlChannelAndSignalInfo, ControlChannelInfo, SignalInterfaceInfo, DeviceInfo>()
                .LeftJoin((a, b, c, d) => a.ChannelId == b.Id)
                .LeftJoin((a, b, c, d) => a.SignalId == c.Id)
                .LeftJoin((a, b, c, d) => a.DeviceId == d.Id)
                .Where((a, b, c, d) => controlChannelIds.Contains(a.ChannelId))
                .ToListAsync((a, b, c, d) => new
                {
                    ChannelId = b.Id,
                    ControlChannelAndSignalInfo = a,
                    SignalInterfaceInfo = c,
                    LinkDeviceInfo = d
                }, cancellationToken);
            var pseudoChannelIds = pseudoChannels.Select(s => s.Id).ToList();
            var pseudoChannelSignals = await freeSql
                .Select<PseudoChannelAndSignalInfo, PseudoChannelInfo, SignalInterfaceInfo>()
                .LeftJoin((a, b, c) => a.PseudoChannelId == b.Id)
                .LeftJoin((a, b, c) => a.SignalId == c.Id) 
                .Where((a, b, c) => pseudoChannelIds.Contains(a.PseudoChannelId))
                .ToListAsync((a, b, c) => new
                {
                    ChannelId = b.Id,
                    PseudoChannelAndSignalInfo = a,
                    SignalInterfaceInfo = c
                }, cancellationToken);
            // 控制通道
            var controlChannelAggregate = new List<ControlChannelBindSignalInfo>();
            foreach (var controlChannel in controlChannels)
            {
                var item = new ControlChannelBindSignalInfo
                {
                    ControlChannelInfo = controlChannel
                };
                var tempSignals = controlChannelSignals.Where(s => s.ChannelId == controlChannel.Id)
                    .Select(s => new ControlChannelSignal
                    {
                        SignalType = s.ControlChannelAndSignalInfo.SignalType,
                        SignalInfo = s.SignalInterfaceInfo,
                        LinkDeviceInfo = s.LinkDeviceInfo,
                    })
                    .ToList();
                item.Signals = tempSignals;
                controlChannelAggregate.Add(item);
            }
            // 虚拟通道
            var pseudoChannelAggregate = new List<PseudoChannelBindSignalInfo>(); 
            foreach (var pseudoChannel in pseudoChannels)
            {
                var item = new PseudoChannelBindSignalInfo
                {
                    PseudoChannelInfo = pseudoChannel
                };
                var tempSignals = pseudoChannelSignals.Where(s => s.ChannelId == pseudoChannel.Id)
                    .Select(s => s.SignalInterfaceInfo)
                    .ToList();
                item.Signals = tempSignals;
                pseudoChannelAggregate.Add(item);
            }

            modelAggregate.BaseInfo = modelBaseInfo;
            if (modelBaseInfo != null)
            {
                modelAggregate.Model3DDataList = await freeSql.Select<Model3DData>()
                    .Where(c => c.GroupKey == modelBaseInfo.Id)
                    .ToListAsync(cancellationToken);
            }
            var aggregate = new StationSiteAggregate
            {
                StationSiteInfo = stationSite,
                ControlChannelSignalInfos = controlChannelAggregate,
                PseudoChannelInfos = pseudoChannelAggregate,
                Signals = signals, 
                Model3DAggregate = modelAggregate
            };
            return aggregate;
        }
    }
}
