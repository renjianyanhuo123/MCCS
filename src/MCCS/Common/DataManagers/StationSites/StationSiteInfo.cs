namespace MCCS.Common.DataManagers.StationSites
{
    public sealed class StationSiteInfo(long id, string name)
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
        public List<StationSiteControlChannelInfo> ControlChannels { get; } = [];
    }
}
