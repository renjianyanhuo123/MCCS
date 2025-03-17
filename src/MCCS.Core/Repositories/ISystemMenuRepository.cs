using MCCS.Core.Models.SystemSetting;

namespace MCCS.Core.Repositories
{
    public interface ISystemMenuRepository
    {
        Task<List<SystemMenu>> GetChildMenusById(long parentId, CancellationToken cancellationToken);

        List<SystemMenu> GetChildMenusById(long parentId);
    }
}
