using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCCS.Models.ControlCommand
{
    public class ManualControlModel
    {
        public required string ChannelId { get; set; }
        public double MaxValue { get; set; }

        public double MinValue { get; set; }

        public double OutputValue { get; set; }
    }
}
