using MCCS.Core.Models.SystemSetting;
using MCCS.Core.Models.TestInfo;
using MCCS.Models;
using MCCS.ViewModels.Pages;

namespace MCCS
{
    public static class SeedData
    {
        public static void InitialData(IFreeSql freeSql)
        {
            if (!freeSql.Select<SystemMenu>().Any())
            {
                var entities = new List<SystemMenu>
                {
                    new()
                    {
                        Key = HomePageViewModel.Tag,
                        Name = "主页",
                        Icon = "Home",
                        Type = MenuType.MainMenu,
                    }
                };
                freeSql.Insert(entities).ExecuteAffrows();
            }
            if (!freeSql.Select<Test>().Any())
            {
                var entities = new List<Test>
                {
                    new()
                    { 
                        Code = "T00001",
                        Name = "试验1",
                        Person="张三",
                        Standard = "ASTM",
                        FilePath = "C:/1.db",
                        Status = TestStatus.Success,
                        StartTime = DateTime.Now,
                        EndTime = DateTime.Now,
                    },
                    new()
                    {
                        Code = "T00002",
                        Name = "试验2",
                        Person="张三11",
                        Standard = "ASTM",
                        FilePath = "C:/1.db",
                        Status = TestStatus.Processing,
                        StartTime = DateTime.Now,
                        EndTime = DateTime.Now,
                    }
                };
                freeSql.Insert(entities).ExecuteAffrows();
            }
        }

    }
}
