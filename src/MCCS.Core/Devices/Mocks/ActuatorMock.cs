using MCCS.Core.Devices.Commands;
using MCCS.Core.Devices.Connections;
using MCCS.Core.Models.Devices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCCS.Core.Devices.Mocks
{
    public class ActuatorMock : BaseDevice
    {
        public ActuatorMock(
            string id,
            string name,
            DeviceTypeEnum type,
            IDeviceConnection connection) : base(id, name, type, connection)
        {
        }

        public override async Task<DeviceData> ReadDataAsync()
        {
            if (_statusSubject.Value != DeviceStatusEnum.Connected)
                throw new InvalidOperationException("Device not connected");
            _statusSubject.OnNext(DeviceStatusEnum.Busy);
            try
            {
                // 模拟读取数据
                await Task.Delay(100); // 模拟延迟
                var data = new DeviceData
                {
                    DeviceId = Id,
                    Value = "Sample Data", // 这里可以替换为实际读取的数据
                    Unit = "units",
                    Timestamp = DateTimeOffset.UtcNow,
                    Metadata = new Dictionary<string, object> { { "source", "simulated" } }
                };
                return data;
            }
            finally
            {
                _statusSubject.OnNext(DeviceStatusEnum.Connected);
            }

        }

        protected override async Task<CommandResponse> ProcessCommandAsync(DeviceCommand command)
        {
            var response = new CommandResponse
            {
                CommandId = command.CommandId,
                DeviceId = Id,
                Success = true
            };
            await Task.Delay(100); // 模拟处理延迟
            return response;
        }
    }
}
