using MCCS.Station.Abstractions.Interfaces;

namespace MCCS.Station.Host
{
    internal class App : IDisposable
    {
        private readonly IStationRuntime _stationRuntime; 

        public App(IStationRuntime stationRuntime)
        {
            _stationRuntime = stationRuntime;
        }

        public async Task RunAsync()
        {
            var stationSiteInfo = await _stationRuntime.InitialStationSiteAsync(DataManager.IsMock);
            // 将站点信息存储到DataManager中以供全局访问
            DataManager.StationSite = stationSiteInfo;
            // 启动共享内存数据发布服务 
        }

        public void StopAsync()
        { 
        }

        public void Dispose()
        {
            StopAsync(); 
        }
    }
}
