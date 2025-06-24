using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCCS.Models
{
    public record CurveMeasureValueModel
    {
        // 可以是时间戳、秒、毫秒等
        public double XValue { get; init; }  
        public double YValue { get; init; }
    }
}
