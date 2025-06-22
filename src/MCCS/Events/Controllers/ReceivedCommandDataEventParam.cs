using MCCS.Models;

namespace MCCS.Events.Controllers
{
    public class ReceivedCommandDataEventParam
    {
        /// <summary>
        /// 需要控制的通道名称的集合
        /// </summary>
        public List<string> ChannelIds { get; set; } = [];
        /// <summary>
        /// 控制模式
        /// </summary>
        public ControlMode ControlMode { get; set; }
        /// <summary>
        /// 控制参数
        /// </summary>
        public required object? Param { get; set; } = null;
    }
}
