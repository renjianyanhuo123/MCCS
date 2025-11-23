using System.Linq.Expressions;
using MCCS.Infrastructure.Models;
using MCCS.Infrastructure.Models.ProjectManager;

namespace MCCS.Infrastructure.Repositories.Project
{
    public interface IProjectRepository
    {

        Task<ProjectModel> GetProjectAsync(long id);

        Task<List<ProjectModel>> GetProjectsAsync(Expression<Func<ProjectModel, bool>> expression);

        Task<PageModel<ProjectModel>> GetPageMethodsAsync(int pageIndex, int pageSize, Expression<Func<ProjectModel, bool>> expression);

        ValueTask<bool> DeleteProjectAsync(long id, CancellationToken cancellationToken = default);
        ValueTask<long> AddProjectAsync(ProjectModel project, CancellationToken cancellationToken = default);

    }
}
