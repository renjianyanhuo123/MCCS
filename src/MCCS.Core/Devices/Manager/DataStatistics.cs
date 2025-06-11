using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCCS.Core.Devices.Manager
{
    public class DataStatistics
    {
        public DateTimeOffset Timestamp { get; set; }
        public int DeviceCount { get; set; }
        public int DataPointsPerSecond { get; set; }
        public int ActiveDevices { get; set; }
    }
}
