using MCCS.Core.Devices.Commands;
using MCCS.Core.Devices.Connections;
using MCCS.Core.Models.Devices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace MCCS.Core.Devices.Details
{
    public sealed class Actuator : BaseDevice
    {
        public Actuator(
            DeviceInfo deviceInfo,
            IDeviceConnection connection)
            : base(
                  deviceInfo.DeviceId, 
                  deviceInfo.DeviceName, 
                  deviceInfo.DeviceType, 
                  connection)
        {
        }

        protected override bool IsDeviceData(byte[] data)
        {
            return true;
        }

        protected override byte[] PrepareCommand(DeviceCommand command)
        {
            return [];
        }

        protected override DeviceData ProcessData(byte[] rawData)
        {
            var str = Encoding.UTF8.GetString(rawData);
            var res = JsonSerializer.Deserialize<MockActuatorCollection>(str);
            var deviceData = new DeviceData() 
            {
                DeviceId = Id,
                Value = res,
                Unit = "none",
                Timestamp = DateTimeOffset.Now,
                Metadata = new Dictionary<string, object>
                {
                    { "rawData", rawData }
                }
            };
            return deviceData;
        }
    }
}
