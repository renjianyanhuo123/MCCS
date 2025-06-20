using MCCS.Core.Devices;
using MCCS.Core.Devices.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCCS.Models
{
    public enum ControlTypeEnum 
    {
        None = 0,
        /// <summary>
        /// 组合控制
        /// </summary>
        Combine = 1,
        /// <summary>
        /// 单一控制
        /// </summary>
        Single = 2
    }

    public enum ControlMode 
    {
        /// <summary>
        /// 手动
        /// </summary>
        Manual,
        /// <summary>
        /// 静态
        /// </summary>
        Static,
        /// <summary>
        /// 程控
        /// </summary>
        Programmable,
        /// <summary>
        /// 疲劳
        /// </summary>
        Fatigue
    } 

    public class ControlInfo
    {
        /// <summary>
        /// 通道ID
        /// </summary>
        public required string ChannelId { get; set; }

        /// <summary>
        /// 通道名称
        /// </summary>
        public required string ChannelName { get; set; }

        /// <summary>
        /// 是否可以控制
        /// </summary>
        public bool IsCanControl { get; set; }

        /// <summary>
        /// 控制类型
        /// </summary>
        public ControlTypeEnum ControlType { get; set; }

        /// <summary>
        /// 控制模式
        /// </summary>
        public ControlMode ControlMode { get; set; }

        /// <summary>
        /// 控制参数
        /// </summary>
        public Dictionary<string, object> ControlParams { get; set; } = []; 

        /// <summary>
        /// 组合控制通道ID
        /// </summary>
        public string? CombineChannelId { get; set; }

        /// <summary>
        /// 组合控制通道名称
        /// </summary>
        public string? CombineChannelName { get; set; }

        /// <summary>
        /// 指令执行状态
        /// </summary>
        public CommandExecuteStatusEnum IsExecuting { get; set; } = CommandExecuteStatusEnum.NoExecute;
        /// <summary>
        /// 控制的设备
        /// </summary>
        public IDevice? Device { get; set; } = null;
    }

    public class ControlCombineInfo
    {
        /// <summary>
        /// 组合控制通道ID
        /// </summary>
        public required string CombineChannelId { get; set; }
        /// <summary>
        /// 组合控制通道名称
        /// </summary>
        public required string CombineChannelName { get; set; }
        /// <summary>
        /// 控制通道列表
        /// </summary>
        public List<ControlInfo> ControlChannels { get; set; } = [];
    }
}
