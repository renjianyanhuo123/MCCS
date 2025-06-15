using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCCS.Core.Devices
{
    public enum CollectionMode
    {
        Continuous, // 连续采集
        OnDemand,   // 按需采集
        Scheduled   // 定时采集
    } 
}
