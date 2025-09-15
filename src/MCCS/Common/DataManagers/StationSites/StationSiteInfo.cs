namespace MCCS.Common.DataManagers.StationSites
{
    public sealed class StationSiteInfo
    {
        public StationSiteInfo(long id, string name)
        {
            Id = id;
            Name = name;
        }
        /// <summary>
        /// 站点ID
        /// </summary>
        public long Id { get; private set; }
        /// <summary>
        /// 站点名称
        /// </summary>
        public string Name { get; private set; }
        /// <summary>
        /// 控制器
        /// </summary>
        public List<StationSiteControllerInfo> ControllerInfos { get; } = []; 
    }
}
