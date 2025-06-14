using MCCS.Core.Devices.Connections;

namespace MCCS.Core.Devices;

public interface IDeviceConnectionFactory
{
    IDeviceConnection CreateConnection(string connectionId, string connectionString, ConnectionTypeEnum type);
}