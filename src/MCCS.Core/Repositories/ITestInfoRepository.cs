using MCCS.Core.Models.TestInfo;
using System.Linq.Expressions;

namespace MCCS.Core.Repositories
{
    public interface ITestInfoRepository
    {
        ValueTask<int> AddEntities(IEnumerable<Test> testInfos, CancellationToken cancellationToken);

        Task<List<Test>> GetTestsAsync(Expression<Func<Test, bool>> expression, CancellationToken cancellationToken);
    }
}
