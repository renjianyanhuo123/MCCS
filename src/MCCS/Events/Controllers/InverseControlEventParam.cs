using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCCS.Events.Controllers
{
    public record InverseControlEventParam
    {
        public required string DeviceId { get; init; }
    }
}
