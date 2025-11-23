using MCCS.Infrastructure.Models.ProjectManager;

namespace MCCS.Infrastructure.Repositories.Project
{
    public interface IProjectDataRecordRepository
    {
        ValueTask<bool> BatchAddRecordAsync(IList<ProjectDataRecordModel> datas, CancellationToken cancellationToken = default);

        ValueTask<bool> AddRecordAsync(ProjectDataRecordModel data, CancellationToken cancellationToken = default);
    }
}
