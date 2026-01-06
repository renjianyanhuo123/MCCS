using Newtonsoft.Json;

namespace MCCS.Station.Abstractions.Models
{
    [method: JsonConstructor]
    public sealed class StationSiteInfo(long id, string name, List<StationSiteControlChannelInfo> controlChannelInfos)
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
        /// 控制通道
        /// </summary>
        public List<StationSiteControlChannelInfo> ControlChannels { get; private set; } = controlChannelInfos;
    }
}
