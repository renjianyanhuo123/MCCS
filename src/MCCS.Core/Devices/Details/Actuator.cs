using MCCS.Core.Devices.Commands;
using MCCS.Core.Devices.Connections;
using MCCS.Core.Models.Devices;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace MCCS.Core.Devices.Details
{
    public sealed class Actuator : BaseDevice
    {

        private static readonly Random _rand = new();

        // 生成符合 N(0, 1) 的随机数（标准正态分布）
        private static double NextStandardNormal()
        {
            var u1 = 1.0 - _rand.NextDouble(); // 避免 log(0)
            var u2 = 1.0 - _rand.NextDouble();
            return Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Cos(2.0 * Math.PI * u2);
        }

        // 生成符合 N(mean, stdDev^2) 的随机数
        private static double NextNormal(double mean, double stdDev)
        {
            return mean + stdDev * NextStandardNormal();
        }
         
        public Actuator(
            DeviceInfo deviceInfo,
            IDeviceConnection connection)
            : base(
                  deviceInfo, 
                  connection)
        {
        }

        public override async Task<CommandResponse> SendCommandAsync(DeviceCommand command)
        {
            var response = new CommandResponse
            {
                CommandId = command.CommandId,
                DeviceId = Id,
                Success = true,
                Result = null,
                ExecutionTime = TimeSpan.Zero,
                Timestamp = DateTimeOffset.Now
            };
            await Task.Delay(10); // 模拟异步操作
            return response;
        }

        protected override async Task<DeviceData> ReadDataAsync()
        {
            var mockForceData = NextNormal(10, 8);
            var mockDisplacement = NextNormal(22, 10);
            var data = new DeviceData
            {
                DeviceId = Id,
                Value = new MockActuatorCollection 
                {
                    Force = mockForceData,
                    Displacement = mockDisplacement
                },
                Unit = "unit",
                Timestamp = DateTimeOffset.Now,
                Metadata = new Dictionary<string, object>
                {
                    { "Mocked", true },
                    { "Description", "This is a mocked actuator data." }
                }
            };
            await Task.Delay(5); // 模拟异步操作 
            return data;
        }
    }
}
