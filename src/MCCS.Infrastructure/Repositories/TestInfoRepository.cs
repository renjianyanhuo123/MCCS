using System.Linq.Expressions;
using MCCS.Infrastructure.Models.TestInfo;

namespace MCCS.Infrastructure.Repositories
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

        public List<Test> GetTests(Expression<Func<Test, bool>> expression)
        {
            return freeSql.Select<Test>()
            .Where(expression)
                .ToList();
        }

        public async Task<List<Test>> GetTestsAsync(Expression<Func<Test, bool>> expression, CancellationToken cancellationToken) 
        {
            return await freeSql.Select<Test>()
                .Where(expression)
                .ToListAsync(cancellationToken);
        }
    }
}
