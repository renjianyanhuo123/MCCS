using FreeSql;
using MCCS.Core.Repositories;
using Microsoft.Extensions.Configuration;
using System.Diagnostics;

namespace MCCS.Modules
{
    public static class RepositoryInject
    {
        public static void AddRepository(this IContainerRegistry containerRegistry, IConfiguration configuration) 
        {
            var connectionStr = configuration["Database:ConnectionString"];
            // 创建 FreeSql 实例
            var freesql = new FreeSqlBuilder()
                .UseConnectionString(DataType.Sqlite, connectionStr)
                .UseAdoConnectionPool(true)
                .UseAutoSyncStructure(true) // 自动同步实体结构到数据库
                .UseMonitorCommand(cmd => Debug.WriteLine($"SQL: {cmd.CommandText}")) // 监视 SQL 命令
                .Build();
            containerRegistry.RegisterInstance(freesql);
            containerRegistry.Register<ISystemMenuRepository, SystemMenuRepository>();
            containerRegistry.Register<ITestInfoRepository, TestInfoRepository>();
            SeedData.InitialData(freesql);
        }
    }
}
