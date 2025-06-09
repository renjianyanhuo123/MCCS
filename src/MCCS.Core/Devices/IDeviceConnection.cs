using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCCS.Core.Devices
{
    public interface IDeviceConnection
    {
        bool IsConnected { get; }
        Task<bool> ConnectAsync();
        Task<bool> DisconnectAsync();
    }
}
