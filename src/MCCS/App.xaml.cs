using MCCS.Modules;
using MCCS.ViewModels.Pages;
using MCCS.Views;
using MCCS.Views.Pages;
using Microsoft.Extensions.Configuration;
using System.IO;
using System.Windows;
using Serilog;
using System.Windows.Threading; 
using MCCS.Core.Repositories;
using MCCS.Core.Devices;
using MCCS.Core.Devices.Connections;
using MCCS.Core.Devices.Manager;
using MCCS.Core.Devices.Collections;

namespace MCCS
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {

        /// <summary>
        /// (3)
        /// </summary>
        /// <returns></returns>
        protected override Window CreateShell()
        {
            return Container.Resolve<MainWindow>();
        }
        /// <summary>
        /// (2)
        /// </summary>
        /// <param name="moduleCatalog"></param>
        protected override void ConfigureModuleCatalog(IModuleCatalog moduleCatalog)
        {
            // 注册模块
            //moduleCatalog.AddModule<RepositoryModule>();
        }
        /// <summary>
        /// (1)
        /// </summary>
        /// <param name="containerRegistry"></param>
        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            // 1. 读取配置文件
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

            // 配置Serilog 日志
            var logger = new LoggerConfiguration()
                .MinimumLevel.Debug() 
                .WriteTo.Async(configure =>
                configure.File("Logs/log.txt", rollingInterval: RollingInterval.Day, 
                    outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}"))
                .CreateLogger();
            // 确保是
            Log.Logger = logger;
            containerRegistry.RegisterInstance<Serilog.ILogger>(logger);
            var configuration = builder.Build();

            // 2. 将 IConfiguration 注册到容器
            containerRegistry.RegisterInstance<IConfiguration>(configuration);
            containerRegistry.AddRepository(configuration);
            containerRegistry.AddModel3DServices(configuration); 

            containerRegistry.RegisterForNavigation<HomePage>(HomePageViewModel.Tag);
            containerRegistry.RegisterForNavigation<HomeTestOperationPage>(HomeTestOperationPageViewModel.Tag);
            containerRegistry.RegisterForNavigation<TestStartingPage>(TestStartingPageViewModel.Tag);
            containerRegistry.Inject(configuration);
        }
        /// <summary>
        /// (4)初始化应用程序
        /// </summary>
        protected override void OnInitialized()
        {
            base.OnInitialized();
        }

        /// <summary>
        /// (0)
        /// </summary>
        /// <param name="e"></param>
        protected override async void OnStartup(StartupEventArgs e)
        {
            // 设置全局异常处理
            SetupExceptionHandling();
            base.OnStartup(e);
            var deviceRepository = Container.Resolve<IDeviceInfoRepository>();
            var devices = await deviceRepository.GetAllDevicesAsync();
            var connectionDevices = devices.Where(c => c.MainDeviceId != null);
            var deviceConnectionFactory = Container.Resolve<IDeviceConnectionFactory>();
            var connectionManager = Container.Resolve<IConnectionManager>();
            var deviceManager = Container.Resolve<IDeviceManager>();
            // 默认注册模拟设备连接
            var connection = deviceConnectionFactory.CreateConnection("Mock", "XXXXX", ConnectionTypeEnum.Mock);
            connectionManager.RegisterConnection(connection);
            // (1)注册所有设备连接
            foreach (var item in connectionDevices)
            {
                var temp = deviceConnectionFactory.CreateConnection(item.MainDeviceId, "XXXXX", ConnectionTypeEnum.TcpIp);
                if (temp != null)
                {
                    connectionManager.RegisterConnection(temp);
                }
                else
                {
                    Log.Error("设备连接失败,设备ID:{deviceId}", item.MainDeviceId);
                } 
            }
            // (2)默认注册所有设备
            await deviceManager.RegisterDeviceFromRepository();
            // (3)注册数据采集器
            var dataCollector = Container.Resolve<IDataCollector>();
            dataCollector.StartCollection(TimeSpan.FromMilliseconds(20));
        }

        protected override void OnExit(ExitEventArgs e)
        {
            Log.CloseAndFlush();
            base.OnExit(e);

        }

        #region private method
        private void SetupExceptionHandling()
        {
            // UI线程异常
            DispatcherUnhandledException += App_DispatcherUnhandledException;

            // 非UI线程异常
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            // Task异常
            TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;
        }

        private void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            Log.Error("全局UI线程异常:{exception}", e.Exception); 
            // 标记异常已处理，防止应用程序崩溃
            e.Handled = true;
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var exception = e.ExceptionObject as Exception;
            Log.Error("非UI线程未处理异常:{exception}", exception);
            if (e.IsTerminating)
            {
                Log.Error("应用程序即将终止:{exception}", exception);
            }
        }

        private void TaskScheduler_UnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
        {
            Log.Error("Task未处理异常:{exception}", e.Exception);
            // 标记异常已处理
            e.SetObserved();
        }
        #endregion
    }

}
