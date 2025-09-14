using System.Linq.Expressions;
using MCCS.Core.Domain;
using MCCS.Core.Models.Model3D;
using MCCS.Core.Models.StationSites;

namespace MCCS.Core.Repositories
{
    public interface IModel3DDataRepository
    {
        Task<List<Model3DData>> GetModelAsync(long groupKey, CancellationToken cancellationToken);

        Task<List<Model3DBaseInfo>> GetModelBaseInfosAsync(Expression<Func<Model3DBaseInfo, bool>> expression, CancellationToken cancellationToken = default);

        List<Model3DBaseInfo> GetModelBaseInfos(Expression<Func<Model3DBaseInfo, bool>> expression); 

        Task<Model3DAggregate?> GetModelAggregateByStationIdAsync(long stationId, CancellationToken cancellationToken = default);

        Task<long> AddModel3DAsync(Model3DBaseInfo baseInfo, List<Model3DData> modelFiles, CancellationToken cancellationToken = default);

        Task<bool> UpdateModel3DAsync(Model3DBaseInfo baseInfo, List<Model3DData> modelFiles, List<ControlChannelAndModel3DInfo> channelsAndModels, CancellationToken cancellationToken = default);
        List<Model3DData> GetModel(long groupKey);
    }
}
