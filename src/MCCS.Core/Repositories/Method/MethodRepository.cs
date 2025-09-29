using MCCS.Core.Models;
using MCCS.Core.Models.MethodManager;
using System.Linq.Expressions;

namespace MCCS.Core.Repositories.Method
{
    public sealed class MethodRepository(IFreeSql freeSql) : IMethodRepository
    {
        public async ValueTask<long> AddMethodAsync(MethodModel method, CancellationToken cancellationToken)
        {
            return await freeSql.Insert(method).ExecuteIdentityAsync(cancellationToken);
        }

        public async ValueTask<bool> DeleteMethodAsync(long id, CancellationToken cancellationToken)
        {
            var rows = await freeSql.Delete<MethodModel>()
                .Where(a => a.Id == id)
                .ExecuteAffrowsAsync(cancellationToken);
            return rows > 0;
        }

        public async Task<List<MethodModel>> GetMethodsAsync(Expression<Func<MethodModel, bool>> expression)
        {
            return await freeSql.Select<MethodModel>()
                .Where(expression)
                .ToListAsync();
        }

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
    }
}
