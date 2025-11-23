using MCCS.Infrastructure.Models.SystemSetting;

namespace MCCS.Infrastructure.Repositories
{
    public interface ISystemMenuRepository
    {
        Task<List<SystemMenu>> GetChildMenusById(long parentId, CancellationToken cancellationToken);

        List<SystemMenu> GetChildMenusById(long parentId);
    }
}
