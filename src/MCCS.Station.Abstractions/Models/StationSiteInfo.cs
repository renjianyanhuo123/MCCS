using Newtonsoft.Json;

namespace MCCS.Station.Abstractions.Models
{
    [method: JsonConstructor]
    public sealed class StationSiteInfo(
        long id, 
        string name, 
        List<StationSiteControlChannelInfo> controlChannels, 
        List<StationSitePseudoChannelInfo> pseudoChannels,
        List<StationSiteControllerSignalInfo> allSignals,
        List<ControllerDevice> controllers,
        List<BaseDevice> allDevices)
    {
        /// <summary>
        /// 站点ID
        /// </summary>
        public long Id { get; private set; } = id;

        /// <summary>
        /// 站点名称
        /// </summary>
        public string Name { get; private set; } = name;

        /// <summary>
        /// 控制通道(逻辑资源)
        /// </summary>
        public List<StationSiteControlChannelInfo> ControlChannels { get; private set; } = controlChannels;

        /// <summary>
        /// 虚拟通道(逻辑资源)
        /// </summary>
        public List<StationSitePseudoChannelInfo> PseudoChannels { get; private set; } = pseudoChannels;

        /// <summary>
        /// 所有的信号接口(物理资源)
        /// </summary>
        public List<StationSiteControllerSignalInfo> AllSignals { get; private set; } = allSignals;

        /// <summary>
        /// 所有控制器(物理资源)
        /// 因为所有的控制都是以控制器展开的，这里相当于唯一的入口
        /// </summary>
        public List<ControllerDevice> Controllers { get; private set; } = controllers;

        /// <summary>
        /// 所有的其他设备(留作备用)(物理资源)
        /// </summary>
        public List<BaseDevice> AllDevices { get; private set; } = allDevices;
    }
}
