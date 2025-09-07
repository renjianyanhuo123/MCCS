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
using MCCS.Core.Models.Devices;
using MCCS.ViewModels.Dialogs;
using MCCS.ViewModels.Dialogs.Common;
using MCCS.ViewModels.Pages.StationSites;
using MCCS.Views.Dialogs;
using MCCS.Views.Pages.SystemManager;
using MCCS.ViewModels.Pages.SystemManager;
using MCCS.Views.Pages.StationSites;
using MCCS.Views.Dialogs.Common;
using MCCS.Views.Dialogs.Hardwares;
using MCCS.ViewModels.Dialogs.Hardwares;
using MCCS.ViewModels.Pages.StationSites.ControlChannels;
using MCCS.Views.Pages.StationSites.ControlChannels;

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
            var shell = Container.Resolve<MainWindow>();
            // 在创建Shell后立即设置全屏
            shell.WindowStyle = WindowStyle.None;
            shell.WindowState = WindowState.Maximized;
            shell.ResizeMode = ResizeMode.NoResize;
            return shell;
        }
        /// <summary>
        /// (2)
        /// </summary>
        /// <param name="moduleCatalog"></param>
        protected override void ConfigureModuleCatalog(IModuleCatalog moduleCatalog)
        {
            // 注册模块
            moduleCatalog.AddModule<NotificationModule>();
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
            containerRegistry.RegisterInstance<ILogger>(logger);
            var configuration = builder.Build();

            // 2. 将 IConfiguration 注册到容器
            containerRegistry.RegisterInstance<IConfiguration>(configuration);
            containerRegistry.AddRepository(configuration);
            containerRegistry.AddModel3DServices(configuration);
            containerRegistry.Inject(configuration);
            // containerRegistry.AddNotificationModule(configuration);
            // containerRegistry.RegisterSingleton<ISharedCommandService, SharedCommandService>();
            containerRegistry.RegisterForNavigation<HomePage>(HomePageViewModel.Tag);
            containerRegistry.RegisterForNavigation<HomeTestOperationPage>(HomeTestOperationPageViewModel.Tag);
            // containerRegistry.RegisterForNavigation<ControllerMainPage>(ControllerMainPageViewModel.Tag);
            containerRegistry.RegisterForNavigation<TestStartingPage>(TestStartingPageViewModel.Tag);
            containerRegistry.RegisterForNavigation<SystemManager>(SystemManagerViewModel.Tag);
            containerRegistry.RegisterForNavigation<PermissionManagement>(PermissionManagementViewModel.Tag);
            containerRegistry.RegisterForNavigation<HardwareSettingPage>(HardwareSettingPageViewModel.Tag);
            containerRegistry.RegisterForNavigation<ChannelSettingPage>(ChannelSettingPageViewModel.Tag);
            containerRegistry.RegisterForNavigation<VariableSettingPage>(VariableSettingPageViewModel.Tag);
            containerRegistry.RegisterForNavigation<AddChannelPage>(AddChannelPageViewModel.Tag);
            containerRegistry.RegisterForNavigation<Model3DSettingPage>(Model3DSettingPageViewModel.Tag);
            containerRegistry.RegisterForNavigation<StationSiteSettingPage>(StationSiteSettingPageViewModel.Tag);
            // Dialogs
            containerRegistry.RegisterDialog<SetCurveDialog>(SetCurveDialogViewModel.Tag);
            containerRegistry.RegisterDialog<AddModel3DDialog>(AddModel3DDialogViewModel.Tag);
            containerRegistry.RegisterDialog<AddStationSiteInfoDialog>(AddStationSiteInfoDialogViewModel.Tag);
            containerRegistry.RegisterDialog<DeleteConfirmDialog>(DeleteConfirmDialogViewModel.Tag);
            containerRegistry.RegisterDialog<AddHardwareDialog>(AddHardwareDialogViewModel.Tag);
            containerRegistry.RegisterDialog<EditHardwareDialog>(EditHardwareDialogViewModel.Tag);
            containerRegistry.RegisterDialog<EditControlChannelPage>(EditControlChannelPageViewModel.Tag);
            // Station Sites
            containerRegistry.RegisterForNavigation<EditStationSiteMainPage>(EditStationSiteMainPageViewModel.Tag);
            containerRegistry.RegisterForNavigation<StationSiteHardwarePage>(StationSiteHardwarePageViewModel.Tag);
            containerRegistry.RegisterForNavigation<StationSiteHydraulicPage>(StationSiteHydraulicPageViewModel.Tag);
            containerRegistry.RegisterForNavigation<StationSiteControlChannelPage>(StationSiteControlChannelPageViewModel.Tag);
            containerRegistry.RegisterForNavigation<StationSitePseudoChannelPage>(StationSitePseudoChannelPageViewModel.Tag);
        }

        /// <summary>
        /// (4)初始化应用程序
        /// </summary>
        protected override async void OnInitialized()
        {
            try
            {
                //var deviceRepository = Container.Resolve<IDeviceInfoRepository>();
                //var devices = await deviceRepository.GetAllDevicesAsync();
                //var connectionDevices = devices.Where(c => c.DeviceType == DeviceTypeEnum.Controller);
                //var connectionManager = Container.Resolve<IConnectionManager>();
                //var deviceManager = Container.Resolve<IDeviceManager>();
                //// (1)默认注册模拟设备连接
                //connectionManager.RegisterBatchConnections(connectionDevices.Select(c => new ConnectionSetting
                //{
                //    ConnectionId = c.DeviceId,
                //    ConnectionType = ConnectionTypeEnum.Mock,
                //    ConnectionStr = "XXXXXX"
                //}).ToList());
                //// (2) 打开所有连接
                //await connectionManager.OpenAllConnections();
                //// (3) 注册所有设备
                //deviceManager.RegisterDevices(devices.Where(c => c.DeviceType == DeviceTypeEnum.Controller).ToList());
                //deviceManager.StartAllDevices();
                base.OnInitialized();
            }
            catch (Exception e)
            {
                Log.Error("数据采集初始化异常:{e.Exception}", e.Message);
            }
        }

        /// <summary>
        /// (0)
        /// </summary>
        /// <param name="e"></param>
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
