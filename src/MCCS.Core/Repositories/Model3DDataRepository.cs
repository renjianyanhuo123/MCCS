using MCCS.Core.Domain;
using MCCS.Core.Models.Model3D;
using System.Linq.Expressions;
using MCCS.Core.Models.StationSites;
using System.Windows.Media.Media3D;

namespace MCCS.Core.Repositories
{
    public class Model3DDataRepository(IFreeSql freeSql) : IModel3DDataRepository
    {
        public async Task<List<Model3DData>> GetModelAsync(long groupKey, CancellationToken cancellationToken)
        {
            return await freeSql.Select<Model3DData>()
                .Where(x => x.GroupKey == groupKey)
                .ToListAsync(cancellationToken);
        }

        public List<Model3DData> GetModel(long groupKey)
        {
            return freeSql.Select<Model3DData>()
                .Where(x => x.GroupKey == groupKey)
                .ToList();
        } 

        public async Task<List<Model3DBaseInfo>> GetModelBaseInfosAsync(Expression<Func<Model3DBaseInfo, bool>> expression, CancellationToken cancellationToken = default)
        {
            return await freeSql.Select<Model3DBaseInfo>()
                .Where(expression)
                .ToListAsync(cancellationToken);
        }

        public async Task<Model3DAggregate> GetModelAggregateAsync(long modelId, CancellationToken cancellationToken = default)
        {
            var modelBase = await freeSql.Select<Model3DBaseInfo>()
                .Where(s => s.Id == modelId)
                .FirstAsync(cancellationToken);
            if (modelBase == null) throw new ArgumentNullException("Model Base is null");
            var modelList = await freeSql.Select<Model3DData>()
                .Where(x => x.GroupKey == modelBase.Id)
                .ToListAsync(cancellationToken);
            var res = new Model3DAggregate
            {
                BaseInfo = modelBase,
                Model3DDataList = modelList
            };
            return res;
        }

        public async Task<long> AddModel3DAsync(Model3DBaseInfo baseInfo, CancellationToken cancellationToken = default)
        {
            var result = await freeSql.Insert(baseInfo)
                .ExecuteIdentityAsync(cancellationToken);
            return result;
        }

        public List<Model3DBaseInfo> GetModelBaseInfos(Expression<Func<Model3DBaseInfo, bool>> expression)
        {
            return freeSql.Select<Model3DBaseInfo>()
                .Where(expression)
                .ToList();
        }

        public async Task<long> AddModel3DAsync(
            Model3DBaseInfo baseInfo, 
            List<Model3DData> modelFiles, 
            List<ControlChannelAndModel3DInfo> controlChannelAndModels, 
            List<ModelBillboardInfo> billboardInfos, 
            CancellationToken cancellationToken = default)
        {
            using var uow = freeSql.CreateUnitOfWork();
            var modelId = await uow.Orm.Insert(baseInfo).ExecuteIdentityAsync(cancellationToken);
            if (modelFiles.Count != 0)
            {
                foreach (var file in modelFiles)
                {
                    file.GroupKey = modelId;
                } 
                foreach (var billboardInfo in billboardInfos)
                {
                    billboardInfo.ModelId = modelId;
                } 
                foreach (var controlChannelAndModel in controlChannelAndModels)
                {
                    controlChannelAndModel.ModelId = modelId;
                }
                if (modelFiles.Count != 0) await uow.Orm.Insert(modelFiles).ExecuteAffrowsAsync(cancellationToken);
                if (billboardInfos.Count != 0) await uow.Orm.Insert(billboardInfos).ExecuteAffrowsAsync(cancellationToken);
                if (controlChannelAndModels.Count != 0) await uow.Orm.Insert(controlChannelAndModels).ExecuteAffrowsAsync(cancellationToken);
            }
            uow.Commit();
            return modelId;
        }

        public async Task<Model3DAggregate?> GetModelAggregateByStationIdAsync(long stationId, CancellationToken cancellationToken = default)
        {
            var modelBase = await freeSql.Select<Model3DBaseInfo>()
                .Where(s => s.StationId == stationId)
                .FirstAsync(cancellationToken);
            if (modelBase == null) return null;
            var modelList = await freeSql.Select<Model3DData>()
                .Where(x => x.GroupKey == modelBase.Id)
                .ToListAsync(cancellationToken);
            var billboardInfos = await freeSql.Select<ModelBillboardInfo>()
                .Where(x => x.ModelId == modelBase.Id)
                .ToListAsync(cancellationToken);
            var res = new Model3DAggregate
            {
                BaseInfo = modelBase,
                Model3DDataList = modelList,
                BillboardInfos = billboardInfos
            };
            return res;
        }

        public async Task<bool> UpdateModel3DAsync(Model3DBaseInfo baseInfo, List<Model3DData> modelFiles, List<ControlChannelAndModel3DInfo> channelsAndModels, List<ModelBillboardInfo> billboardInfos, CancellationToken cancellationToken = default)
        {
            using var uow = freeSql.CreateUnitOfWork();
            var count1 = await uow.Orm.Update<Model3DBaseInfo>()
                .Set(c => c.CameraBackgroundColor, baseInfo.CameraBackgroundColor)
                .Set(c => c.CameraLookDirection, baseInfo.CameraLookDirection)
                .Set(c => c.CameraPosition, baseInfo.CameraPosition)
                .Set(c => c.CameraUpDirection, baseInfo.CameraUpDirection)
                .Set(c => c.Description, baseInfo.Description)
                .Set(c => c.FarPlaneDistance, baseInfo.FarPlaneDistance)
                .Set(c => c.NearPlaneDistance, baseInfo.NearPlaneDistance)
                .Set(c => c.FieldViewWidth, baseInfo.FieldViewWidth)
                .Set(c => c.MaterialColor, baseInfo.MaterialColor) 
                .Set(c => c.Name, baseInfo.Name)
                .Where(c => c.Id == baseInfo.Id)
                .ExecuteAffrowsAsync(cancellationToken);
            var count2 = await uow.Orm.Delete<Model3DData>()
                .Where(c => c.GroupKey == baseInfo.Id).ExecuteAffrowsAsync(cancellationToken);
            var count3 = await uow.Orm.Delete<ControlChannelAndModel3DInfo>()
                .Where(c => c.ModelId == baseInfo.Id)
                .ExecuteAffrowsAsync(cancellationToken);
            var count4 = await uow.Orm.Delete<ModelBillboardInfo>()
                .Where(c => c.ModelId == baseInfo.Id)
                .ExecuteAffrowsAsync(cancellationToken);
            await uow.Orm.Insert(modelFiles).ExecuteAffrowsAsync(cancellationToken);
            await uow.Orm.Insert(channelsAndModels).ExecuteAffrowsAsync(cancellationToken);
            await uow.Orm.Insert(billboardInfos).ExecuteAffrowsAsync(cancellationToken);
            uow.Commit();
            return true;
        } 
    }
}
