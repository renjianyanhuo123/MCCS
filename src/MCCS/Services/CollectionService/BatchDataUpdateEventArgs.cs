using MCCS.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCCS.Services.CollectionService
{
    public class BatchDataUpdateEventArgs : EventArgs
    {
        public Dictionary<string, SensorData> UpdatedData { get; set; }
        public DateTime BatchTimestamp { get; set; }
    }
}
