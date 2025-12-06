using MCCS.Common.DataManagers.CurrentTest;
using MCCS.Common.DataManagers.Devices;
using MCCS.Common.DataManagers.Model3Ds;
using MCCS.Common.DataManagers.StationSites; 
using MCCS.Infrastructure.Models.Devices;

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
        /// 所有设备
        /// </summary>
        public List<BaseDevice>? Devices { get; private set; }
        /// <summary>
        /// 控制器 核心
        /// </summary>
        public List<ControllerDevice>? ControllerInfos { get; private set; }  
        /// <summary>
        /// 当前正在进行的试验
        /// </summary>
        public CurrentTestInfo CurrentTestInfo { get; private set; } = new();

        /// <summary>
        /// 当前使用的3D模型集合
        /// </summary>
        public List<Model3DMainInfo>? Model3Ds { get; private set; } 

        public void SetValue<T>(T value) where T : class
        {
            switch (value)
            {
                case StationSiteInfo stationSiteInfo:
                    StationSiteInfo = stationSiteInfo;
                    break; 
                case CurrentTestInfo currentTestInfo:
                    CurrentTestInfo = currentTestInfo;
                    break;
                case List<BaseDevice> devices:
                    Devices = devices;
                    ControllerInfos = devices.Where(c => c.Type == DeviceTypeEnum.Controller)
                        .Select(s => (ControllerDevice)s).ToList();
                    break;
                case List<Model3DMainInfo> model3Ds:
                    Model3Ds = model3Ds;
                    break;
            }
        }
    }
}
