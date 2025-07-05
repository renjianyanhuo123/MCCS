using System.Collections.Concurrent;

namespace MCCS.Core.Devices.Connections
{
    public sealed class ConnectionManager : IConnectionManager
    {
        private readonly ConcurrentDictionary<string, IDeviceConnection> _connections = new();
        private readonly IDeviceConnectionFactory _deviceConnectionFactory;

        public ConnectionManager(IDeviceConnectionFactory deviceConnectionFactory) 
        {
            _deviceConnectionFactory = deviceConnectionFactory ?? throw new ArgumentNullException(nameof(deviceConnectionFactory));
        }

        public void RegisterConnection(ConnectionSetting connectionSetting)
        {
            var connection = _deviceConnectionFactory.CreateConnection(connectionSetting.ConnectionId, connectionSetting.ConnectionStr, connectionSetting.ConnectionType);
            _connections.TryAdd(connectionSetting.ConnectionId, connection);
        }

        public void RegisterBatchConnections(List<ConnectionSetting> connectionSettings) 
        {
            foreach (var setting in connectionSettings)
            {
                RegisterConnection(setting);
            }
        }

        public async Task OpenAllConnections()
        {
            foreach (var connection in _connections.Values)
            {
                await connection.OpenAsync();
            }
        }

        public IDeviceConnection? GetConnection(string connectionId)
        {
            return _connections.TryGetValue(connectionId, out var connection) ? connection : null;
        }

        public void RemoveConnection(string connectionId)
        {
            if (_connections.TryRemove(connectionId, out var connection))
            {
                connection.Dispose();
            }
        }

        public void Dispose()
        {
            foreach (var connection in _connections.Values)
            {
                connection.Dispose();
            }
            _connections.Clear();
        }
    }
}
