using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCCS.Core.Devices
{
    public class MockDeviceConnection : IDeviceConnection
    {
        public bool IsConnected { get; private set; }

        public async Task<bool> ConnectAsync()
        {
            await Task.Delay(1000); // Simulate a delay for connection
            IsConnected = true; // Simulate a successful connection
            return true; // Simulate a successful connection
        }

        public async Task<bool> DisconnectAsync()
        {
            await Task.Delay(1000); // Simulate a delay for disconnection
            IsConnected = false; // Simulate a successful disconnection
            return true; // Simulate a successful disconnection
        }
    }
}
