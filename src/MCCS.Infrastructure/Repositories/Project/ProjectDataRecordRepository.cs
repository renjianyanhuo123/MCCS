using MCCS.Infrastructure.DbContexts;
using MCCS.Infrastructure.Models.ProjectManager;

namespace MCCS.Infrastructure.Repositories.Project
{
    public sealed class ProjectDataRecordRepository(IProjectDbContext projectDbContext) : IProjectDataRecordRepository
    {
        private readonly IProjectDbContext _projectDbContext = projectDbContext;

        public async ValueTask<bool> BatchAddRecordAsync(IList<ProjectDataRecordModel> datas, CancellationToken cancellationToken)
        {
            var freeSql = _projectDbContext.GetDbFreeSql();
            using var uow = freeSql.CreateUnitOfWork();  
            var signalItems = new List<ProjectSignalItemModel>();
            foreach (var data in datas)
            {
                signalItems.AddRange(data.SignalItems);
            }
            await uow.Orm.Insert(datas).ExecuteAffrowsAsync(cancellationToken).ConfigureAwait(false);
            await uow.Orm.Insert(signalItems).ExecuteAffrowsAsync(cancellationToken).ConfigureAwait(false); ;
            uow.Commit();
            return true;
        }

        public async ValueTask<bool> AddRecordAsync(ProjectDataRecordModel data, CancellationToken cancellationToken)
        {
            var freeSql = _projectDbContext.GetDbFreeSql();
            using var uow = freeSql.CreateUnitOfWork(); 
            await uow.Orm.Insert(data).ExecuteAffrowsAsync(cancellationToken).ConfigureAwait(false);
            await uow.Orm.Insert(data.SignalItems).ExecuteAffrowsAsync(cancellationToken).ConfigureAwait(false); ;
            uow.Commit();
            return true;
        }
    }
}
