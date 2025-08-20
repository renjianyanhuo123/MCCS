using MCCS.Core.Domain;
using MCCS.Core.Models.Model3D;
using System.Linq.Expressions;

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

        public async Task<Model3DAggregate> GetCurrentUseModelAggregateAsync(CancellationToken cancellationToken = default)
        {
            var modelBase = await freeSql.Select<Model3DBaseInfo>()
                .Where(x => x.IsUse)
                .FirstAsync(cancellationToken);
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
    }
}
