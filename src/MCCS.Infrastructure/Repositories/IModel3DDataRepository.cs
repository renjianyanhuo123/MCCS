using System.Linq.Expressions; 
using MCCS.Infrastructure.Domain;
using MCCS.Infrastructure.Models.Model3D;
using MCCS.Infrastructure.Models.StationSites;

namespace MCCS.Infrastructure.Repositories
{
    public interface IModel3DDataRepository
    {
        Task<List<Model3DData>> GetModelAsync(long groupKey, CancellationToken cancellationToken);

        Task<List<Model3DBaseInfo>> GetModelBaseInfosAsync(Expression<Func<Model3DBaseInfo, bool>> expression, CancellationToken cancellationToken = default);

        List<Model3DBaseInfo> GetModelBaseInfos(Expression<Func<Model3DBaseInfo, bool>> expression); 

        Task<Model3DAggregate?> GetModelAggregateByStationIdAsync(long stationId, CancellationToken cancellationToken = default);

        Task<long> AddModel3DAsync(Model3DBaseInfo baseInfo, List<Model3DData> modelFiles, List<ControlChannelAndModel3DInfo> controlChannelAndModels, List<ModelBillboardInfo> billboardInfos, CancellationToken cancellationToken = default);
        
        Task<bool> UpdateModel3DAsync(Model3DBaseInfo baseInfo, List<Model3DData> modelFiles, List<ControlChannelAndModel3DInfo> channelsAndModels, List<ModelBillboardInfo> billboardInfos, CancellationToken cancellationToken = default);
        List<Model3DData> GetModel(long groupKey);
    }
}
