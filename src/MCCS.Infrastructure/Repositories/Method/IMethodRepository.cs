using System.Linq.Expressions;
using MCCS.Infrastructure.Models;
using MCCS.Infrastructure.Models.MethodManager;

namespace MCCS.Infrastructure.Repositories.Method
{
    public interface IMethodRepository
    {
        Task<MethodModel> GetMethodAsync(long id);

        Task<List<MethodModel>> GetMethodsAsync(Expression<Func<MethodModel, bool>> expression);

        Task<PageModel<MethodModel>> GetPageMethodsAsync(int pageIndex, int pageSize, Expression<Func<MethodModel, bool>> expression);

        ValueTask<bool> DeleteMethodAsync(long id, CancellationToken cancellationToken = default);
        ValueTask<long> AddMethodAsync(MethodModel method, CancellationToken cancellationToken = default);
    }
}
