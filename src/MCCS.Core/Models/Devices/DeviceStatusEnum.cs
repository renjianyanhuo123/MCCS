using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCCS.Core.Models.Devices
{
    public enum DeviceStatusEnum: int
    {
        /// <summary>
        /// 未知
        /// </summary>
        Unknown = 0,
        /// <summary>
        /// 在线
        /// </summary>
        Online = 1,
        /// <summary>
        /// 离线
        /// </summary>
        Offline = 2,
        /// <summary>
        /// 错误 异常
        /// </summary>
        Error = 3,
        /// <summary>
        /// 维护中
        /// </summary>
        Maintenance = 4,
        /// <summary>
        /// 无效
        /// </summary>
        Disabled = 5
    }
}
