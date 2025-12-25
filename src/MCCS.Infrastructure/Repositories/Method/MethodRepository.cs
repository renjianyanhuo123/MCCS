using System.Linq.Expressions;
using MCCS.Infrastructure.DbContexts;
using MCCS.Infrastructure.Models;
using MCCS.Infrastructure.Models.MethodManager;

namespace MCCS.Infrastructure.Repositories.Method
{
    public sealed class MethodRepository(IFreeSql<SystemDbFlag> freeSql) : IMethodRepository
    {
        public async ValueTask<long> AddMethodAsync(MethodModel method, CancellationToken cancellationToken) => 
            await freeSql.Insert(method).ExecuteIdentityAsync(cancellationToken);

        public async ValueTask<bool> DeleteMethodAsync(long id, CancellationToken cancellationToken)
        {
            var rows = await freeSql.Delete<MethodModel>()
                .Where(a => a.Id == id)
                .ExecuteAffrowsAsync(cancellationToken);
            return rows > 0;
        }

        public Task<MethodInterfaceSettingModel> GetInterfaceSettingAsync(long methodId) =>
            freeSql.Select<MethodInterfaceSettingModel>()
                .Where(c => c.MethodId == methodId)
                .ToOneAsync();

        public Task<MethodModel> GetMethodAsync(long id) =>
            freeSql.Select<MethodModel>()
                .Where(c => c.Id == id)
                .ToOneAsync();

        public Task<List<MethodModel>> GetMethodsAsync(Expression<Func<MethodModel, bool>> expression) =>
            freeSql.Select<MethodModel>()
                .Where(expression)
                .ToListAsync();

        public Task<MethodUiComponentsModel> GetMethodUiComponentByIdAsync(long componentId) =>
            freeSql.Select<MethodUiComponentsModel>()
                .Where(c => c.Id == componentId)
                .ToOneAsync();

        public async Task<PageModel<MethodModel>> GetPageMethodsAsync(int pageIndex, int pageSize, Expression<Func<MethodModel, bool>> expression)
        {
            var res = new PageModel<MethodModel>
            {
                TotalCount = await freeSql.Select<MethodModel>().Where(expression)
                    .CountAsync(),
                Items = await freeSql.Select<MethodModel>()
                    .Where(expression) 
                    .Page(pageIndex, pageSize)
                    .ToListAsync()
            };
            return res;
        }

        public Task<List<MethodUiComponentsModel>> GetUiComponentsAsync() =>
            freeSql.Select<MethodUiComponentsModel>()
                .ToListAsync();
    }
}
