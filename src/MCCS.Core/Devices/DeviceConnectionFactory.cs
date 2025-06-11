using MCCS.Core.Devices.Connections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCCS.Core.Devices
{
    public class DeviceConnectionFactory : IDeviceConnectionFactory
    {
        public IDeviceConnection CreateConnection(string connectionString, ConnectionTypeEnum type)
        {
            switch (type)
            {
                case ConnectionTypeEnum.SerialPort:
                    return new SerialPortConnection("COM1");
                case ConnectionTypeEnum.TcpIp:
                    break;
                case ConnectionTypeEnum.Modbus:
                    break;
                case ConnectionTypeEnum.OPC:
                    break;
                default:
                    break;
            }
            return new MockDeviceConnection(connectionString);
        }
    }
}
