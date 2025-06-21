using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCCS.Models.ControlCommand
{
    public class StaticControlModel
    {
        public required string ChannelId { get; set; }
        public ControlUnitTypeEnum UnitType { get; set; }

        public double Speed { get; set; }

        public double TargetValue { get; set; }
    }
}
