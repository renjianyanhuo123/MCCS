using MCCS.Infrastructure.Communication.NamedPipe;
using MCCS.Station.Abstractions.Interfaces;
using MCCS.Station.Host.Handlers;

namespace MCCS.Station.Host
{
    internal class App : IDisposable
    {
        private readonly IStationRuntime _stationRuntime;
        private readonly IDataPublisher _dataPublisher;
        private readonly NamedPipeServer _namedPipeServer;

        public App(
            IStationRuntime stationRuntime,
            IDataPublisher dataPublisher,
            IServiceProvider serviceProvider)
        {
            _stationRuntime = stationRuntime;
            _dataPublisher = dataPublisher;
            // 创建服务端并自动注册处理器（支持依赖注入）
            _namedPipeServer = NamedPipeFactory.CreateServerFromAttributes(
                serviceProvider: serviceProvider,
                maxConnections: 20,
                assemblies: [typeof(CommandHandler).Assembly]);
        }

        public async Task RunAsync(CancellationToken cancellationToken)
        {
            var stationSiteInfo = await _stationRuntime.InitialStationSiteAsync(DataManager.IsMock, cancellationToken);
            // 将站点信息存储到DataManager中以供全局访问
            DataManager.StationSite = stationSiteInfo;
            // 启动共享内存数据发布服务
            await _dataPublisher.StartAsync(cancellationToken);
            // 启动命名管道服务器
            await _namedPipeServer.StartAsync();

        }

        public void StopAsync()
        {
            _namedPipeServer.Dispose();
        }

        public void Dispose() => StopAsync();
    }
}
