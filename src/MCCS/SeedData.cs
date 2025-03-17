using MCCS.Core.Models.SystemSetting;
using MCCS.ViewModels.Pages;
using System.Diagnostics;

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
                Debug.WriteLine("初始化数据已插入");
            }
        }
    }
}
