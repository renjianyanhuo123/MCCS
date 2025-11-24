using MCCS.Infrastructure.DbContexts;
using MCCS.Infrastructure.Models.SystemSetting;

namespace MCCS.Infrastructure.Repositories
{
    public class SystemMenuRepository(IFreeSql<SystemDbFlag> freeSql) : ISystemMenuRepository
    {
        public async Task<List<SystemMenu>> GetChildMenusById(long parentId, CancellationToken cancellationToken) 
        {
            return await freeSql.Select<SystemMenu>()
                .Where(x => x.ParentId == parentId)
                .ToListAsync(cancellationToken);
        }

        public List<SystemMenu> GetChildMenusById(long parentId)
        {
            return freeSql.Select<SystemMenu>()
                .Where(x => x.ParentId == parentId)
                .ToList();
        }
    }
}
