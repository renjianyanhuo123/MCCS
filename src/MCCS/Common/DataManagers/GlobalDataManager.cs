using MCCS.Common.DataManagers.CurrentTest;
using MCCS.Common.DataManagers.Model3Ds;
using MCCS.Infrastructure.Helper;
using MCCS.Station.Abstractions.Models;

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

        public ProcessManager? ProcessManager { get; private set; }
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
                    break;
                case List<Model3DMainInfo> model3Ds: 
                    break;
                case Infrastructure.Helper.ProcessManager processManager:
                    ProcessManager = processManager;
                    break;
            }
        }
    }
}
