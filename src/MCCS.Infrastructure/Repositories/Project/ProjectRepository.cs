using System.Linq.Expressions;
using MCCS.Infrastructure.DbContexts;
using MCCS.Infrastructure.Models;
using MCCS.Infrastructure.Models.ProjectManager;

namespace MCCS.Infrastructure.Repositories.Project
{
    public class ProjectRepository(IFreeSql<SystemDbFlag> freeSql) : IProjectRepository
    {
        public Task<ProjectModel> GetProjectAsync(long id)
        {
             return freeSql.Select<ProjectModel>()
                 .Where(s => s.IsDeleted == false && s.Id == id)
                 .ToOneAsync();
        }

        public Task<List<ProjectModel>> GetProjectsAsync(Expression<Func<ProjectModel, bool>> expression)
        {
            return freeSql.Select<ProjectModel>()
                .Where(expression)
                .ToListAsync();
        }

        public async Task<PageModel<ProjectModel>> GetPageMethodsAsync(int pageIndex, int pageSize, Expression<Func<ProjectModel, bool>> expression)
        {
            var res = new PageModel<ProjectModel>
            {
                TotalCount = await freeSql.Select<ProjectModel>().Where(expression)
                    .CountAsync(),
                Items = await freeSql.Select<ProjectModel>()
                    .Where(expression)
                    .Page(pageIndex, pageSize)
                    .ToListAsync()
            };
            return res;
        }

        public async ValueTask<bool> DeleteProjectAsync(long id, CancellationToken cancellationToken = default)
        {
            var rows = await freeSql.Delete<ProjectModel>()
                .Where(a => a.Id == id)
                .ExecuteAffrowsAsync(cancellationToken);
            return rows > 0;
        }

        public async ValueTask<long> AddProjectAsync(ProjectModel project, CancellationToken cancellationToken = default)
        {
            return await freeSql.Insert(project).ExecuteIdentityAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}
