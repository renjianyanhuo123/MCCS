using MCCS.Modules;
using MCCS.ViewModels.Pages;
using MCCS.Views;
using MCCS.Views.Pages;
using Microsoft.Extensions.Configuration;
using System.IO;
using System.Windows;
using Serilog;
using System.Windows.Threading;

namespace MCCS
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {

        /// <summary>
        /// (2)
        /// </summary>
        /// <returns></returns>
        protected override Window CreateShell()
        {
            return Container.Resolve<MainWindow>();
        }
        /// <summary>
        /// (3)
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

            
        }
        
        protected override void OnStartup(StartupEventArgs e)
        {
            // 设置全局异常处理
            SetupExceptionHandling();
            base.OnStartup(e);
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
