using MCCS.Core.Devices.Commands;
using MCCS.Core.Devices.Connections;
using MCCS.Core.Models.Devices;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace MCCS.Core.Devices.Details
{
    public sealed class Actuator : BaseDevice
    {

        private static readonly Random _rand = new();
        private double _currentForce = 10.0; // 初始力值
        private double _currentDisplacement = 0.0; // 初始位移值5mm
        private DateTime _currentStartTime = DateTime.MinValue;

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
        private async Task CommonReduceGapLogic(
            Func<double> getValue, 
            Action<double> setValue, 
            double targetValue, 
            double speed, 
            string commandId,
            CancellationToken cancellationToken)
        {
            var lastGap = double.MaxValue;
            while (true)
            {
                var operationValue = getValue();
                var currentGap = Math.Abs(operationValue - targetValue);
                if (currentGap >= lastGap) break;

                lastGap = currentGap;
                var newValue = operationValue + Math.Sign(targetValue - operationValue) * speed / 60.0;
                setValue(newValue);
                _commandStatusSubject.OnNext(new CommandResponse
                {
                    CommandId = commandId,
                    DeviceId = Id,
                    Success = true,
                    Result = null,
                    Progress = operationValue / targetValue * 1.0,
                    CommandExecuteStatus = CommandExecuteStatusEnum.Executing,
                    ExecutionTime = (DateTime.Now - _currentStartTime),
                    Timestamp = DateTimeOffset.Now
                });
                await Task.Delay(1000, cancellationToken);
            }
            _commandStatusSubject.OnNext(new CommandResponse
            {
                CommandId = commandId,
                DeviceId = Id,
                Success = true,
                Result = null,
                Progress = 1.0,
                CommandExecuteStatus = CommandExecuteStatusEnum.ExecuttionCompleted,
                ExecutionTime = (DateTime.Now - _currentStartTime),
                Timestamp = DateTimeOffset.Now
            });
        }

        public override async Task<CommandResponse> SendCommandAsync(DeviceCommand command, CancellationToken cancellationToken = default)
        {
            var response = new CommandResponse
            {
                CommandId = command.CommandId,
                DeviceId = Id,
                Success = true,
                Result = null,
                CommandExecuteStatus = CommandExecuteStatusEnum.Executing,
                ExecutionTime = TimeSpan.Zero,
                Timestamp = DateTimeOffset.Now
            };
            _currentStartTime = DateTime.Now;
            _commandStatusSubject.OnNext(response);
            if (command.Type == CommandTypeEnum.SetMove) 
            {
                var parameters = command.Parameters ?? [];
                var unit = Convert.ToInt32(parameters.GetValueOrDefault("UnitType"));
                var speed = Convert.ToDouble(parameters.GetValueOrDefault("Speed"));
                var targetV = Convert.ToDouble(parameters.GetValueOrDefault("TargetValue"));
                _ = Task.Run(async () =>
                {
                    Func<double> operation = unit == 0 ? () => _currentForce : () => _currentDisplacement;
                    Action<double> target = unit == 0 ?
                        (value) => _currentForce = value :
                        (value) => _currentDisplacement = value;
                    await CommonReduceGapLogic(operation, target, targetV, speed, command.DeviceId, cancellationToken);
                }, cancellationToken);
            }
            await Task.Delay(10, cancellationToken); // 模拟异步操作
            return response;
        }

        protected override async Task<DeviceData> ReadDataAsync()
        {
            var mockForceData = NextNormal(_currentForce, 0.006);
            var mockDisplacement = NextNormal(_currentDisplacement, 0.006);
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
