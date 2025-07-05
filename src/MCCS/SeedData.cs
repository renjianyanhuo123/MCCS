using MCCS.Core.Models.Devices;
using MCCS.Core.Models.Model3D;
using MCCS.Core.Models.SystemSetting;
using MCCS.Core.Models.TestInfo;
using MCCS.ViewModels.Pages;
using Newtonsoft.Json;

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
                        FilePath = @"F:\\models\\test\\main.STL",
                        PositionStr = "0,0,0",
                        Description = "0",
                        Type = ModelType.Other,
                        RotationStr = "1,0,0",
                        ScaleStr = "0.005,0.005,0.005",
                        RotateAngle = -90.0
                    },
                    new()
                    {
                        Key = Guid.NewGuid().ToString("N"),
                        Name = "模型2",
                        GroupKey = "2025052901",
                        FilePath = @"F:\\models\\test\\1.STL",
                        Description = "1",
                        PositionStr = "0,0,0",
                        Type = ModelType.Other,
                        RotationStr = "1,0,0",
                        ScaleStr = "0.005,0.005,0.005",
                        RotateAngle = -90.0
                    },
                    new()
                    {
                        Key = Guid.NewGuid().ToString("N"),
                        Name = "模型3",
                        GroupKey = "2025052901",
                        FilePath = @"F:\\models\\test\\2.STL",
                        Description = "2",
                        PositionStr = "0,0,0",
                        Type = ModelType.Other,
                        RotationStr = "1,0,0",
                        ScaleStr = "0.005,0.005,0.005",
                        RotateAngle = -90.0
                    },
                    new()
                    {
                        Key = Guid.NewGuid().ToString("N"),
                        Name = "模型4",
                        GroupKey = "2025052901",
                        FilePath = @"F:\\models\\test\\3.STL",
                        Description = "3",
                        PositionStr = "0,0,0",
                        Type = ModelType.Actuator,
                        RotationStr = "1,0,0",
                        ScaleStr = "0.005,0.005,0.005",
                        RotateAngle = -90.0,
                        DeviceId = "1882785835a24a83aae53ac51f153c04",
                        Orientation = "0,-1,0"
                    },
                    new()
                    {
                        Key = Guid.NewGuid().ToString("N"),
                        Name = "模型5",
                        GroupKey = "2025052901",
                        FilePath = @"F:\\models\\test\\4.STL",
                        Description = "4",
                        PositionStr = "0,0,0",
                        Type = ModelType.Other,
                        RotationStr = "1,0,0",
                        ScaleStr = "0.005,0.005,0.005",
                        RotateAngle = -90.0,
                    },
                    new()
                    {
                        Key = Guid.NewGuid().ToString("N"),
                        Name = "模型6",
                        GroupKey = "2025052901",
                        FilePath = @"F:\\models\\test\\5.STL",
                        Description = "5",
                        PositionStr = "0,0,0",
                        Type = ModelType.Other,
                        RotationStr = "1,0,0",
                        ScaleStr = "0.005,0.005,0.005",
                        RotateAngle = -90.0
                    },
                    new()
                    {
                        Key = Guid.NewGuid().ToString("N"),
                        Name = "模型7",
                        GroupKey = "2025052901",
                        FilePath = @"F:\\models\\test\\6.STL",
                        Description = "6",
                        PositionStr = "0,0,0",
                        Type = ModelType.Other,
                        RotationStr = "1,0,0",
                        ScaleStr = "0.005,0.005,0.005",
                        RotateAngle = -90.0
                    },
                    new()
                    {
                        Key = Guid.NewGuid().ToString("N"),
                        Name = "模型8",
                        GroupKey = "2025052901",
                        FilePath = @"F:\\models\\test\\7.STL",
                        Description = "7",
                        PositionStr = "0,0,0",
                        Type = ModelType.Other,
                        RotationStr = "1,0,0",
                        ScaleStr = "0.005,0.005,0.005",
                        RotateAngle = -90.0
                    },
                    new()
                    {
                        Key = Guid.NewGuid().ToString("N"),
                        Name = "模型9",
                        GroupKey = "2025052901",
                        FilePath = @"F:\\models\\test\\8.STL",
                        Description = "8",
                        PositionStr = "0,0,0",
                        Type = ModelType.Other,
                        RotationStr = "1,0,0",
                        ScaleStr = "0.005,0.005,0.005",
                        RotateAngle = -90.0
                    },
                    new()
                    {
                        Key = Guid.NewGuid().ToString("N"),
                        Name = "模型10",
                        GroupKey = "2025052901",
                        FilePath = @"F:\\models\\test\\9.STL",
                        Description = "9",
                        PositionStr = "0,0,0",
                        Type = ModelType.Other,
                        RotationStr = "1,0,0",
                        ScaleStr = "0.005,0.005,0.005",
                        RotateAngle = -90.0
                    },
                    new()
                    {
                        Key = Guid.NewGuid().ToString("N"),
                        Name = "模型11",
                        GroupKey = "2025052901",
                        FilePath = @"F:\\models\\test\\10.STL",
                        Description = "10",
                        PositionStr = "0,0,0",
                        Type = ModelType.Other,
                        RotationStr = "1,0,0",
                        ScaleStr ="0.005,0.005,0.005",
                        RotateAngle = -90.0
                    },
                    new()
                    {
                        Key = Guid.NewGuid().ToString("N"),
                        Name = "模型12",
                        GroupKey = "2025052901",
                        FilePath = @"F:\\models\\test\\11.STL",
                        Description = "11",
                        PositionStr = "0,0,0",
                        Type = ModelType.Other,
                        RotationStr = "1,0,0",
                        ScaleStr = "0.005,0.005,0.005",
                        RotateAngle = -90.0
                    },
                    new()
                    {
                        Key = Guid.NewGuid().ToString("N"),
                        Name = "模型13",
                        GroupKey = "2025052901",
                        FilePath = @"F:\\models\\test\\12.STL",
                        Description = "12",
                        PositionStr = "0,0,0",
                        Type = ModelType.Actuator,
                        RotationStr = "1,0,0",
                        ScaleStr = "0.005,0.005,0.005",
                        RotateAngle = -90.0,
                        DeviceId = "dd1d1798920e49cbaba9b4a59aead10f",
                        Orientation = "1,0,0"
                    }
                };
                freeSql.Insert(entities).ExecuteAffrows();
            }
            if (!freeSql.Select<DeviceInfo>().Any()) 
            {
                var mainDeviceId1 = Guid.NewGuid().ToString("N");
                var devices = new List<DeviceInfo>() 
                {
                    new() {
                        DeviceId = Guid.NewGuid().ToString("N"),
                        DeviceName = "作动器1",
                        DeviceType = DeviceTypeEnum.Actuator,
                        Description = "作动器1的传感器数据",
                        MainDeviceId = mainDeviceId1,
                        ConnectionInfo = JsonConvert.SerializeObject(new Dictionary<string, string>{ { "port", "COM3" } }),
                    },
                    new() {
                        DeviceId = Guid.NewGuid().ToString("N"),
                        DeviceName = "作动器2",
                        DeviceType = DeviceTypeEnum.Actuator,
                        Description = "作动器2的传感器数据",
                        MainDeviceId = mainDeviceId1,
                        ConnectionInfo = JsonConvert.SerializeObject(new Dictionary<string, string>{ { "port", "COM1" } }),
                    },
                    new() {
                        DeviceId = mainDeviceId1,
                        DeviceName = "控制器1",
                        DeviceType = DeviceTypeEnum.Controller,
                        Description = "控制器",
                        ConnectionInfo = JsonConvert.SerializeObject(new Dictionary<string, string>{ { "ip", "192.168.0.112" } }),
                    }
                };
                freeSql.Insert(devices).ExecuteAffrows();
            }
        }

    }
}
