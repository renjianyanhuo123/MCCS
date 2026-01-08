using MCCS.Station.Abstractions.Interfaces;
using MCCS.Station.Core;
using MCCS.Station.Core.ControlChannelManagers;
using MCCS.Station.Core.ControllerManagers;
using MCCS.Station.Core.PseudoChannelManagers;
using MCCS.Station.Core.SignalManagers;

using Microsoft.Extensions.DependencyInjection;

namespace MCCS.Station.Host
{
    internal class Program
    { 
        private static readonly CancellationTokenSource _cts = new();

        private static void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<IControllerManager, ControllerManager>();
            services.AddSingleton<ISignalManager, SignalManager>();
            services.AddSingleton<IControlChannelManager, ControlChannelManager>();
            services.AddSingleton<IPseudoChannelManager, PseudoChannelManager>();
            services.AddTransient<IStationRuntime, StationRuntime>();
            services.AddSingleton<App>();
        }

        static void ReadCommands()
        {
            while (true)
            {
                var line = Console.ReadLine();
                if (line?.Trim().ToLower() != "stop") continue;
#if DEBUG
                Console.WriteLine("Stop command received.");
#endif
                Cleanup();
                break;
            }
        }

        static async Task Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            // 解析命令行参数(是否为MOCK模式)
            foreach (var item in args)
            {
                DataManager.IsMock = item?.ToLower() == "true";
            }
            // 创建服务容器
            var serviceCollection = new ServiceCollection(); 
            // 注册服务
            ConfigureServices(serviceCollection);
            // 构建服务提供者
            var serviceProvider = serviceCollection.BuildServiceProvider();

            // 获取并使用服务
            var app = serviceProvider.GetRequiredService<App>();
            await app.RunAsync();
            _ = Task.Run(ReadCommands);
            // 保持主线程运行，直到收到取消请求
            while (!_cts.Token.IsCancellationRequested)
            {
                await Task.Delay(2000, _cts.Token);
            }
        } 

        /// <summary>
        /// 清理资源
        /// </summary>
        private static void Cleanup()
        {
            _cts.Cancel();
            _cts.Dispose();
#if DEBUG
            Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] 资源清理完成");
#endif
        }
    }
}
