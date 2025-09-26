using System.Linq.Expressions;
using MCCS.Core.Models.MethodManager;

namespace MCCS.Core.Repositories.Method
{
    public interface IMethodRepository
    {
        Task<List<MethodModel>> GetMethodsAsync(Expression<Func<MethodModel, bool>> expression);
    }
}
