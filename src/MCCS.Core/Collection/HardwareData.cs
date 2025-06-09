using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCCS.Core.Collection
{
    /// <summary>
    /// 硬件接口数据结构
    /// </summary>
    public class HardwareData
    {
        public string ActuatorId { get; set; }
        public double ForceValue { get; set; }
        public double DisplacementValue { get; set; }
        public bool IsValid { get; set; }
    }
}
