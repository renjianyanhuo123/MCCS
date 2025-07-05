using MCCS.Core.Devices.Connections;

namespace MCCS.Core.Devices
{
    public sealed class DeviceConnectionFactory : IDeviceConnectionFactory
    {
        public IDeviceConnection CreateConnection(string connectionId, string connectionString, ConnectionTypeEnum type)
        {
            return type switch
            {
                ConnectionTypeEnum.SerialPort => new SerialPortConnection(connectionId, "COM1"),
                ConnectionTypeEnum.TcpIp => throw new NotSupportedException(),
                ConnectionTypeEnum.Modbus => throw new NotSupportedException(),
                ConnectionTypeEnum.OPC => throw new NotSupportedException(),
                _ => new MockDeviceConnection(connectionString, connectionId),
            };
        }
    }
}
