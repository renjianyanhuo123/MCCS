using FreeSql;

namespace MCCS.Infrastructure.DbContexts
{
    public sealed class ProjectDbContext : IProjectDbContext
    { 
        private IFreeSql? _freeSql;

        public void Initial(string dbStr)
        {
            _freeSql?.Dispose();
            _freeSql = new FreeSqlBuilder()
                .UseConnectionString(DataType.Sqlite, $"Data Source={dbStr}")
                .UseAutoSyncStructure(true)
                .Build(); 
        }

        public IFreeSql GetDbFreeSql()
        {
            if (_freeSql == null) throw new ArgumentNullException("must initial db");
            return _freeSql;
        }
    }
}
