using MCCS.Core.Models.Model3D;

namespace MCCS.Core.Repositories
{
    public interface IModel3DDataRepository
    {
        public Task<List<Model3DData>> GetModelAsync(string groupKey);

        public List<Model3DData> GetModel(string groupKey);
    }
}
