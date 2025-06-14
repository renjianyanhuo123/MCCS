using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCCS.Core.Devices.Connections
{
    public interface IConnectionManager : IDisposable
    {
        void RegisterConnection(IDeviceConnection connection);

        IDeviceConnection? GetConnection(string connectionId);

        void RemoveConnection(string connectionId);
    }
}
