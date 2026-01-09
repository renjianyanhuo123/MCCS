using MCCS.Station.Abstractions.Communication;
using MCCS.Station.Abstractions.Interfaces;
using MCCS.Station.Host.Communication;

namespace MCCS.Station.Host
{
    internal class App : IAsyncDisposable
    {
        private readonly IStationRuntime _stationRuntime;
        private readonly SharedMemoryDataPublisher _dataPublisher;

        public App(IStationRuntime stationRuntime, SharedMemoryDataPublisher dataPublisher)
        {
            _stationRuntime = stationRuntime;
            _dataPublisher = dataPublisher;
        }

        public async Task RunAsync()
        {
            var stationSiteInfo = await _stationRuntime.InitialStationSiteAsync(DataManager.IsMock);
            // 将站点信息存储到DataManager中以供全局访问
            DataManager.StationSite = stationSiteInfo;

            // 启动共享内存数据发布服务
            await _dataPublisher.StartAsync();

#if DEBUG
            Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Application Running!");
            Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Shared memory data publisher started");
            Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Data channel: {SharedMemoryDataPublisher.DataChannelName}");
            Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Status channel: {SharedMemoryDataPublisher.StatusChannelName}");
#endif
        }

        public async Task StopAsync()
        {
            await _dataPublisher.StopAsync();
#if DEBUG
            var stats = _dataPublisher.GetStatistics();
            Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Publisher statistics:");
            Console.WriteLine($"  - Total packets published: {stats.TotalPacketsPublished}");
            Console.WriteLine($"  - Failed publish count: {stats.FailedPublishCount}");
            Console.WriteLine($"  - Uptime: {stats.Uptime}");
#endif
        }

        public async ValueTask DisposeAsync()
        {
            await StopAsync();
            _dataPublisher.Dispose();
        }
    }
}
