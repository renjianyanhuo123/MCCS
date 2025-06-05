using MCCS.Core.Models.Model3D;

namespace MCCS.Core.Repositories
{
    public class Model3DDataRepository(IFreeSql freeSql) : IModel3DDataRepository
    {
        public async Task<List<Model3DData>> GetModelAsync(string groupKey, CancellationToken cancellationToken)
        {
            return await freeSql.Select<Model3DData>()
                .Where(x => x.GroupKey == groupKey)
                .ToListAsync(cancellationToken);
        }

        public List<Model3DData> GetModel(string groupKey)
        {
            return freeSql.Select<Model3DData>()
                .Where(x => x.GroupKey == groupKey)
                .ToList();
        }
    }
}
