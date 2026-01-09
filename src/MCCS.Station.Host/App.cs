using MCCS.Station.Abstractions.Interfaces;

namespace MCCS.Station.Host
{
    internal class App : IDisposable
    {
        private readonly IStationRuntime _stationRuntime; 
        private readonly IDataPublisher _dataPublisher;

        public App(IStationRuntime stationRuntime, IDataPublisher dataPublisher)
        {
            _stationRuntime = stationRuntime;
            _dataPublisher = dataPublisher;
        }

        public async Task RunAsync(CancellationToken cancellationToken)
        {
            var stationSiteInfo = await _stationRuntime.InitialStationSiteAsync(DataManager.IsMock, cancellationToken);
            // 将站点信息存储到DataManager中以供全局访问
            DataManager.StationSite = stationSiteInfo; 
            // 启动共享内存数据发布服务 
            await _dataPublisher.StartAsync(cancellationToken);
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
