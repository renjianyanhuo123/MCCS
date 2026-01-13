using MCCS.Infrastructure.Communication.NamedPipe;
using MCCS.Infrastructure.Communication.NamedPipe.Models;
using MCCS.Station.Abstractions.Interfaces;

namespace MCCS.Station.Host
{
    internal class App : IDisposable
    {
        private readonly IStationRuntime _stationRuntime; 
        private readonly IDataPublisher _dataPublisher;
        private readonly NamedPipeServer _namedPipeServer;

        public App(IStationRuntime stationRuntime, IDataPublisher dataPublisher)
        {
            _stationRuntime = stationRuntime;
            _dataPublisher = dataPublisher;
            // 创建服务端
            _namedPipeServer = NamedPipeFactory.CreateServer("MCCS_Command_IPC", maxConnections: 10);
        }

        public async Task RunAsync(CancellationToken cancellationToken)
        {
            var stationSiteInfo = await _stationRuntime.InitialStationSiteAsync(DataManager.IsMock, cancellationToken);
            // 将站点信息存储到DataManager中以供全局访问
            DataManager.StationSite = stationSiteInfo; 
            // 启动共享内存数据发布服务 
            await _dataPublisher.StartAsync(cancellationToken);
            // 注册处理器
            _namedPipeServer.RegisterHandler("echo", request =>
                PipeResponse.Success(request.RequestId, request.Payload));

            _namedPipeServer.RegisterHandler("calculate/add", (req, ct) => { 
                return Task.FromResult(PipeResponse.Success(req.RequestId, ""));
            });

            await _namedPipeServer.StartAsync();

        }

        public void StopAsync()
        {
            _namedPipeServer.Dispose();
        }

        public void Dispose() => StopAsync();
    }
}
