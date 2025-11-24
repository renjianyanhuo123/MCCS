using System.Diagnostics;
using FreeSql;

namespace MCCS.Infrastructure.DbContexts
{
    public class ProjectDbFlag{}

    public sealed class ProjectDbContext : IProjectDbContext
    { 
        private IFreeSql<ProjectDbFlag>? _freeSql;

        public void Initial(string dbStr)
        {
            _freeSql?.Dispose();
            _freeSql = new FreeSqlBuilder()
                .UseConnectionString(DataType.Sqlite, $"Data Source={dbStr}")
                .UseAdoConnectionPool(true)
                .UseAutoSyncStructure(true) // 自动同步实体结构到数据库
                .UseMonitorCommand(cmd => Debug.WriteLine($"Project SQL: {cmd.CommandText}")) // 监视 SQL 命令 
                .Build<ProjectDbFlag>(); 
        }

        public IFreeSql<ProjectDbFlag> GetDbFreeSql()
        {
            if (_freeSql == null) throw new ArgumentNullException("must initial db");
            return _freeSql;
        }
    }
}
