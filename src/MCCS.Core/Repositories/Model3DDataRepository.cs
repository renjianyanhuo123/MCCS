using MCCS.Core.Models.Model3D;

namespace MCCS.Core.Repositories
{
    public class Model3DDataRepository(IFreeSql freeSql) : IModel3DDataRepository
    {
        public async Task<List<Model3DData>> GetModelAsync(string groupKey)
        {
            return await freeSql.Select<Model3DData>()
                .Where(x => x.GroupKey == groupKey)
                .ToListAsync();
        }

        public List<Model3DData> GetModel(string groupKey)
        {
            return freeSql.Select<Model3DData>()
                .Where(x => x.GroupKey == groupKey)
                .ToList();
        }
    }
}
