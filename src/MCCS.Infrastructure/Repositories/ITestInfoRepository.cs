using System.Linq.Expressions;
using MCCS.Infrastructure.Models.TestInfo;

namespace MCCS.Infrastructure.Repositories
{
    public interface ITestInfoRepository
    {
        ValueTask<int> AddEntities(IEnumerable<Test> testInfos, CancellationToken cancellationToken);

        Task<List<Test>> GetTestsAsync(Expression<Func<Test, bool>> expression, CancellationToken cancellationToken);

        List<Test> GetTests(Expression<Func<Test, bool>> expression);
    }
}
