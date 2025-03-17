using MCCS.Core.Models.SystemSetting;

namespace MCCS.Core.Repositories
{
    public class SystemMenuRepository(IFreeSql freeSql) : ISystemMenuRepository
    {
        private readonly IFreeSql _freeSql = freeSql;

        public async Task<List<SystemMenu>> GetChildMenusById(long parentId, CancellationToken cancellationToken) 
        {
            return await _freeSql.Select<SystemMenu>()
                .Where(x => x.ParentId == parentId)
                .ToListAsync();
        }

        public List<SystemMenu> GetChildMenusById(long parentId)
        {
            return _freeSql.Select<SystemMenu>()
                .Where(x => x.ParentId == parentId)
                .ToList();
        }
    }
}
