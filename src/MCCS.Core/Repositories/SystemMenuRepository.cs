using MCCS.Core.Models.SystemSetting;

namespace MCCS.Core.Repositories
{
    public class SystemMenuRepository(IFreeSql freeSql) : ISystemMenuRepository
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
