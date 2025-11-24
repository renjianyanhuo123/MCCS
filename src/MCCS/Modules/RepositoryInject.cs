using FreeSql; 
using Microsoft.Extensions.Configuration;
using System.Diagnostics;
using MCCS.Infrastructure.DbContexts;
using MCCS.Infrastructure.Repositories;
using MCCS.Infrastructure.Repositories.Method;
using MCCS.Infrastructure.Repositories.Project;

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
                .Build<SystemDbFlag>();
            containerRegistry.RegisterInstance<IFreeSql<SystemDbFlag>>(freesql);
            containerRegistry.RegisterSingleton<IProjectDbContext, ProjectDbContext>();
            containerRegistry.Register<ISystemMenuRepository, SystemMenuRepository>();
            containerRegistry.Register<ITestInfoRepository, TestInfoRepository>();
            containerRegistry.Register<IModel3DDataRepository, Model3DDataRepository>();
            containerRegistry.Register<IDeviceInfoRepository, DeviceInfoRepository>(); 
            containerRegistry.Register<IStationSiteAggregateRepository, StationSiteAggregateRepository>();
            containerRegistry.Register<ICurveAggregateRepository, CurveAggregateRepository>();
            containerRegistry.Register<IProjectDataRecordRepository, ProjectDataRecordRepository>();

            containerRegistry.Register<IMethodRepository, MethodRepository>();
            containerRegistry.Register<IProjectRepository, ProjectRepository>();
            SeedData.InitialData(freesql);
        }
    }
}
