namespace MCCS.Infrastructure.DbContexts;

public interface IProjectDbContext
{
    /// <summary>
    /// 初始化操作
    /// </summary>
    /// <param name="dbStr"></param>
    void Initial(string dbStr);
    /// <summary>
    /// 获取数据库操作
    /// </summary>
    /// <returns></returns>
    IFreeSql GetDbFreeSql();
}