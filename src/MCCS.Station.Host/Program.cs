using MCCS.Station.Abstractions.Interfaces;
using MCCS.Station.Core;
using MCCS.Station.Core.ControlChannelManagers;
using MCCS.Station.Core.ControllerManagers;
using MCCS.Station.Core.PseudoChannelManagers;
using MCCS.Station.Core.SignalManagers;
using MCCS.Station.Host.Communication;
using MCCS.Station.Host.Services;
using MCCS.Station.Services;

using Microsoft.Extensions.DependencyInjection;

namespace MCCS.Station.Host
{
    internal class Program
    {
        private static readonly CancellationTokenSource _cts = new();
        private static App? _app;
        private static ServiceProvider? _serviceProvider;

        private static void ConfigureServices(IServiceCollection services)
        {
            // 核心服务
            services.AddSingleton<IControllerManager, ControllerManager>();
            services.AddSingleton<ISignalManager, SignalManager>();
            services.AddSingleton<IControlChannelManager, ControlChannelManager>();
            services.AddSingleton<IPseudoChannelManager, PseudoChannelManager>();
            services.AddTransient<IStationRuntime, StationRuntime>();

            // 通信服务
            services.AddSingleton<SharedMemoryDataPublisher>();
            services.AddSingleton<IDataPublisher>(sp => sp.GetRequiredService<SharedMemoryDataPublisher>()); 
            // 命令服务
            services.AddCommandServiceCollection(); 
            // 应用程序
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
            _serviceProvider = serviceCollection.BuildServiceProvider();

            // 获取并使用服务
            _app = _serviceProvider.GetRequiredService<App>();

            try
            {
                await _app.RunAsync(_cts.Token);
                _ = Task.Run(ReadCommands);

                // 保持主线程运行，直到收到取消请求
                while (!_cts.Token.IsCancellationRequested)
                {
                    await Task.Delay(2000, _cts.Token);
                }
            }
            catch (OperationCanceledException)
            {
                // 正常取消
            }
            catch (Exception ex)
            {
#if DEBUG
                Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Error: {ex.Message}");
#endif
            }
        }

        /// <summary>
        /// 异步清理资源
        /// </summary>
        private static void Cleanup()
        {
            // 停止应用程序
            _app?.Dispose();
            // 取消令牌
            _cts.Cancel();
            _cts.Dispose(); 
            // 释放服务提供者
            _serviceProvider?.Dispose();

#if DEBUG
            Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] 资源清理完成");
#endif
        }
    }
}
