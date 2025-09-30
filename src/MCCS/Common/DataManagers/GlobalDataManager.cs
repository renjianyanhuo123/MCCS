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
        /// 当前编辑的方法信息
        /// </summary>
        public MethodContentItemModel? MethodInfo { get; private set; }
        /// <summary>
        /// 所有硬件的连接状态
        /// </summary>
        public List<IControllerHardwareDevice>? HardwareDevices { get; private set; }

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
                case List<IControllerHardwareDevice> hardwareDevices:
                    HardwareDevices = hardwareDevices;
                    break;
            }
        }
    }
}
