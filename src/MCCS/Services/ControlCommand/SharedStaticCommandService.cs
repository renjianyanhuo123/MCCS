using MCCS.Models;
using MCCS.Models.ControlCommand;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCCS.Services.ControlCommand
{
    public class SharedStaticCommandService : ISharedStaticCommandService
    {
        public ControlUnitTypeEnum UnitType { get; set; }

        public double Speed { get; set; }

        public double TargetValue { get; set; }
    }
}
