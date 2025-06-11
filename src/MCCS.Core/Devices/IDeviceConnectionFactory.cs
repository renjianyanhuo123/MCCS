using MCCS.Core.Devices.Connections;

namespace MCCS.Core.Devices;

public interface IDeviceConnectionFactory
{
    IDeviceConnection CreateConnection(string connectionString, ConnectionTypeEnum type);
}