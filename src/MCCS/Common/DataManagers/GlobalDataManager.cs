using MCCS.Collecter.HardwareDevices;
using MCCS.Common.DataManagers.Methods;
using MCCS.Common.DataManagers.StationSites;

namespace MCCS.Common.DataManagers
{
    internal sealed class GlobalDataManager
    {
        static GlobalDataManager() { }
        
        private static readonly Lazy<GlobalDataManager> _lazy = new(() => new GlobalDataManager(), LazyThreadSafetyMode.ExecutionAndPublication);

        public static GlobalDataManager Instance => _lazy.Value;
        /// <summary>
        /// 当前启用的站点所有聚合信息
        /// </summary>
        public StationSiteInfo? StationSiteInfo { get; private set; }
        /// <summary>
        /// 控制器 核心
        /// </summary>
        public List<StationSiteControllerInfo>? ControllerInfos { get; private set; } 
        /// <summary>
        /// 当前编辑的方法信息
        /// </summary>
        public MethodContentItemModel? MethodInfo { get; private set; } 

        public void SetValue<T>(T value) where T : class
        {
            switch (value)
            {
                case StationSiteInfo stationSiteInfo:
                    StationSiteInfo = stationSiteInfo;
                    break;
                case MethodContentItemModel methodContentItemModel:
                    MethodInfo = methodContentItemModel;
                    break;
                case List<StationSiteControllerInfo> controllers:
                    ControllerInfos = controllers;
                    break;
            }
        }
    }
}
