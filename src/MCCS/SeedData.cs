using MCCS.Core.Models.Model3D;
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
                    },
                    new()
                    {
                        Key = TestStartingPageViewModel.Tag,
                        Name = "开始试验",
                        Icon = "PlayCircle",
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
                        Status = TestStatus.Success
                    },
                    new()
                    {
                        Code = "T00002",
                        Name = "试验2",
                        Person="张三11",
                        Standard = "ASTM",
                        FilePath = "C:/1.db",
                        Status = TestStatus.Failed
                    },
                    new()
                    {
                        Code = "T00003",
                        Name = "试验3",
                        Person="张三11",
                        Standard = "ASTM",
                        FilePath = "C:/2.db",
                        Status = TestStatus.Processing
                    }
                };
                freeSql.Insert(entities).ExecuteAffrows();
            }
            if (!freeSql.Select<Model3DData>().Any())
            {
                var entities = new List<Model3DData>
                {
                    new()
                    {
                        Key = Guid.NewGuid().ToString("N"),
                        Name = "模型1",
                        GroupKey = "2025052901",
                        FilePath = @"F:\\models\\others\\model1.stl",
                        PositionStr = "1,0,0",
                        Description = "这是一个测试模型",
                        Type = ModelType.Actuator,
                    },
                    new()
                    {
                        Key = Guid.NewGuid().ToString("N"),
                        Name = "模型2",
                        GroupKey = "2025052901",
                        FilePath = @"F:\\models\\others\\model1.stl",
                        Description = "这是另一个测试模型",
                        PositionStr = "0,0,0",
                        Type = ModelType.Other
                    }
                };
                freeSql.Insert(entities).ExecuteAffrows();
            }
        }

    }
}
