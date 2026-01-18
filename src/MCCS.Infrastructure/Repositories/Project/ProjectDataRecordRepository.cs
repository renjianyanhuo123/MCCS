using MCCS.Infrastructure.DbContexts;
using MCCS.Infrastructure.Models.ProjectManager;

namespace MCCS.Infrastructure.Repositories.Project
{
    public sealed class ProjectDataRecordRepository(IProjectDbContext projectDbContext) : IProjectDataRecordRepository
    {
        public async ValueTask<bool> BatchAddRecordAsync(List<ProjectDataRecordModel> datas, CancellationToken cancellationToken)
        {
            var freeSql = projectDbContext.GetDbFreeSql(); 
            var signalItems = new List<ProjectDataRecordItemModel>();
            foreach (var data in datas)
            {
                signalItems.AddRange(data.Items);
            }
            await freeSql.Insert(datas).ExecuteAffrowsAsync(cancellationToken);
            await freeSql.Insert(signalItems).ExecuteAffrowsAsync(cancellationToken);
            return true;
        }

        public async ValueTask<bool> AddRecordAsync(ProjectDataRecordModel data, CancellationToken cancellationToken)
        {
            var freeSql = projectDbContext.GetDbFreeSql();
            using var uow = freeSql.CreateUnitOfWork(); 
            await uow.Orm.Insert(data).ExecuteAffrowsAsync(cancellationToken);
            await uow.Orm.Insert(data.Items).ExecuteAffrowsAsync(cancellationToken);
            uow.Commit();
            return true;
        }
    }
}
