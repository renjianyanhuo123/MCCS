using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCCS.Core.Collection
{
    public interface IHardwareInterface
    {
        Task<List<HardwareData>> ReadSensorDataAsync(CancellationToken cancellationToken);
        Task<bool> ConnectAsync();
        Task DisconnectAsync();
        bool IsConnected { get; }
    }
}
