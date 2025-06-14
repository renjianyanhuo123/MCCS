using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCCS.Core.Devices.Connections
{
    public sealed class ConnectionManager : IConnectionManager
    {
        private readonly ConcurrentDictionary<string, IDeviceConnection> _connections = new();

        public void RegisterConnection(IDeviceConnection connection)
        {
            _connections.TryAdd(connection.ConnectionId, connection);
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
