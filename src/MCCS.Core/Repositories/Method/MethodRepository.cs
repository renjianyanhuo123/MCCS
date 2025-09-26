using MCCS.Core.Models.MethodManager;
using System.Linq.Expressions;

namespace MCCS.Core.Repositories.Method
{
    public sealed class MethodRepository(IFreeSql freeSql) : IMethodRepository
    {
        public async Task<List<MethodModel>> GetMethodsAsync(Expression<Func<MethodModel, bool>> expression)
        {
            return await freeSql.Select<MethodModel>()
                .Where(expression)
                .ToListAsync();
        }
    }
}
