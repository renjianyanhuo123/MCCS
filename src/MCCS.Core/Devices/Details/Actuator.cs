using MCCS.Core.Devices.Commands;
using MCCS.Core.Devices.Connections;
using MCCS.Core.Models.Devices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCCS.Core.Devices.Details
{
    public sealed class Actuator : BaseDevice
    {
        public Actuator(
            DeviceInfo deviceInfo,
            IDeviceConnectionFactory connectionFactory,
            bool isMock = true)
            : base(
                  deviceInfo.DeviceId, 
                  deviceInfo.DeviceName, 
                  deviceInfo.DeviceType, 
                  connectionFactory,
                  isMock)
        {
        }

        public override async Task<DeviceData> ReadDataAsync()
        {
            if (_statusSubject.Value != DeviceStatusEnum.Connected)
                throw new InvalidOperationException("Device not connected");
            _statusSubject.OnNext(DeviceStatusEnum.Busy);
            try
            {
                // TODO: 修改为实际的命令和数据处理逻辑
                var command = new byte[] { 0x01, 0x03, 0x00, 0x00, 0x00, 0x01 };
                var data = await _connection.SendCommandAsync(command);
                var res = new DeviceData
                {
                    DeviceId = Id,
                    Value = data,
                    Unit = "units",
                    Timestamp = DateTimeOffset.UtcNow,
                    Metadata = new Dictionary<string, object> { { "source", "simulated" } }
                };
                return res;
            }
            finally
            {
                _statusSubject.OnNext(DeviceStatusEnum.Connected);
            }

        }

        /// <summary>
        /// 在BaseDevice中实现的命令处理方法
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        protected override async Task<CommandResponse> ProcessCommandAsync(DeviceCommand command)
        {
            var response = new CommandResponse
            {
                CommandId = command.CommandId,
                DeviceId = Id,
                Success = true
            };
            // TODO: 实现具体的命令处理逻辑
            await Task.Delay(100); // 模拟处理延迟
            return response;
        }
    }
}
