using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCCS.Core.Devices.Manager
{
    public class DataCollectionError
    {
        public string DeviceId { get; set; }
        public Exception Error { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
