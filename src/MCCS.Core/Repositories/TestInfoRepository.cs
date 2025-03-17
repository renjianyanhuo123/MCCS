using MCCS.Core.Models.TestInfo;
using System.Linq.Expressions;
namespace MCCS.Core.Repositories
{
    public class TestInfoRepository(IFreeSql freeSql) : ITestInfoRepository
    {
        public async ValueTask<int> AddEntities(IEnumerable<Test> testInfos, CancellationToken cancellationToken) 
        {
            return await freeSql
                .Insert(testInfos)
                .NoneParameter()
                .ExecuteAffrowsAsync(cancellationToken);
        }

        public async Task<List<Test>> GetTestsAsync(Expression<Func<Test, bool>> expression, CancellationToken cancellationToken) 
        {
            return await freeSql.Select<Test>()
                .Where(expression)
                .ToListAsync(cancellationToken);
        }
    }
}
