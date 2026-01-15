using System.IO;
using System.Windows;
using System.Windows.Threading;

using DryIoc.Microsoft.DependencyInjection;

using MCCS.Common.DataManagers;
using MCCS.Common.Resources.ViewModels;
using MCCS.Common.Resources.Views;
using MCCS.Infrastructure.WorkflowSettings;
using MCCS.Interface.Components;
using MCCS.Modules;
using MCCS.Services.AppExitService;
using MCCS.Services.StartInitial;
using MCCS.Station.Client;
using MCCS.ViewModels.Dialogs;
using MCCS.ViewModels.Dialogs.Hardwares;
using MCCS.ViewModels.Dialogs.Method;
using MCCS.ViewModels.Dialogs.Project;
using MCCS.ViewModels.MethodManager;
using MCCS.ViewModels.MethodManager.Contents;
using MCCS.ViewModels.Pages;
using MCCS.ViewModels.Pages.StationSetup;
using MCCS.ViewModels.Pages.StationSites;
using MCCS.ViewModels.Pages.StationSites.ControlChannels;
using MCCS.ViewModels.Pages.StationSites.PseudoChannels;
using MCCS.ViewModels.Pages.StationSteup;
using MCCS.ViewModels.Pages.SystemManager;
using MCCS.ViewModels.Pages.TestModelOperations;
using MCCS.ViewModels.ProjectManager;
using MCCS.Views;
using MCCS.Views.Dialogs;
using MCCS.Views.Dialogs.Hardwares;
using MCCS.Views.Dialogs.Method;
using MCCS.Views.Dialogs.Project;
using MCCS.Views.MethodManager;
using MCCS.Views.MethodManager.Contents; 
using MCCS.Views.Pages;
using MCCS.Views.Pages.StationSetup;
using MCCS.Views.Pages.StationSites;
using MCCS.Views.Pages.StationSites.ControlChannels;
using MCCS.Views.Pages.StationSites.PseudoChannels;
using MCCS.Views.Pages.SystemManager;
using MCCS.Views.Pages.TestModelOperations;
using MCCS.Views.ProjectManager;
using MCCS.Workflow.StepComponents;
using MCCS.WorkflowSetting;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Serilog;

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
            moduleCatalog.AddModule<WorkflowModule>();
            moduleCatalog.AddModule<StepComponentModule>();
            moduleCatalog.AddModule<InterfaceModule>();
            moduleCatalog.AddModule<StationModule>();
        }


        /// <summary>
        /// (1)
        /// </summary>
        /// <param name="containerRegistry"></param>
        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            // 获取当前环境
            var environment = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") ?? "";
            // 1. 读取配置文件
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{environment}.json", optional: true, reloadOnChange: true);

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
            var configuration = builder
                .AddEnvironmentVariables()
                .Build(); 
            // 创建 IServiceCollection
            var container = containerRegistry.GetContainer();
            var services = new ServiceCollection();
            services.AddWorkflowSteps(configuration); 
            container.Populate(services);
            // 2. 将 IConfiguration 注册到容器
            containerRegistry.RegisterInstance<IConfiguration>(configuration);
            containerRegistry.AddRepository(configuration);
            containerRegistry.AddModel3DServices(configuration);
            containerRegistry.Inject(configuration);
            
            containerRegistry.RegisterSingleton<ISplashService, SplashService>();
            containerRegistry.RegisterSingleton<IAppExitService, AppExitService>();
            // containerRegistry.AddNotificationModule(configuration);
            // containerRegistry.RegisterSingleton<ISharedCommandService, SharedCommandService>();
            containerRegistry.RegisterForNavigation<SplashPage>(SplashPageViewModel.Tag);
            containerRegistry.RegisterForNavigation<MainContentPage>(MainContentPageViewModel.Tag);
            containerRegistry.RegisterForNavigation<HomePage>(HomePageViewModel.Tag);
            containerRegistry.RegisterForNavigation<HomeTestOperationPage>(HomeTestOperationPageViewModel.Tag);
            // Right Menu Test Operation
            containerRegistry.RegisterForNavigation<RightMenuMainPage>(RightMenuMainPageViewModel.Tag);

            containerRegistry.RegisterForNavigation<TestStartingPage>(TestStartingPageViewModel.Tag);
            containerRegistry.RegisterForNavigation<SystemManager>(SystemManagerViewModel.Tag);
            containerRegistry.RegisterForNavigation<PermissionManagement>(PermissionManagementViewModel.Tag);
            containerRegistry.RegisterForNavigation<HardwareSettingPage>(HardwareSettingPageViewModel.Tag);
            containerRegistry.RegisterForNavigation<ChannelSettingPage>(ChannelSettingPageViewModel.Tag);
            containerRegistry.RegisterForNavigation<VariableSettingPage>(VariableSettingPageViewModel.Tag);
            containerRegistry.RegisterForNavigation<AddChannelPage>(AddChannelPageViewModel.Tag);
            containerRegistry.RegisterForNavigation<Model3DSettingPage>(Model3DSettingPageViewModel.Tag);
            containerRegistry.RegisterForNavigation<StationSiteSettingPage>(StationSiteSettingPageViewModel.Tag);
            containerRegistry.RegisterForNavigation<ProjectOperationPage>(nameof(ProjectOperationPageViewModel));

            // Project-Components 
            // Dialogs
            // containerRegistry.RegisterDialogWindow<NonModalDialogWindow>("NonModalWindow");
            containerRegistry.RegisterDialog<SetCurveDialog>(SetCurveDialogViewModel.Tag);
            containerRegistry.RegisterDialog<AddModel3DDialog>(AddModel3DDialogViewModel.Tag);
            containerRegistry.RegisterDialog<AddStationSiteInfoDialog>(AddStationSiteInfoDialogViewModel.Tag);
            containerRegistry.RegisterDialog<DeleteConfirmDialog>(nameof(DeleteConfirmDialogViewModel));
            containerRegistry.RegisterDialog<AddHardwareDialog>(AddHardwareDialogViewModel.Tag);
            containerRegistry.RegisterDialog<EditHardwareDialog>(EditHardwareDialogViewModel.Tag);
            containerRegistry.RegisterDialog<EditControlChannelPage>(EditControlChannelPageViewModel.Tag);
            containerRegistry.RegisterDialog<AddPseudoChannelPage>(AddPseudoChannelPageViewModel.Tag);
            containerRegistry.RegisterDialog<EditPseudoChannelPage>(EditPseudoChannelPageViewModel.Tag);
            // Dialog  Methods
            containerRegistry.RegisterDialog<AddMethodDialog>(AddMethodDialogViewModel.Tag);
            // Dialog Projects
            containerRegistry.RegisterDialog<AddProjectDialog>(AddProjectDialogViewModel.Tag); 
            containerRegistry.RegisterDialog<ProjectContentDialog>(nameof(ProjectContentDialogViewModel));
            // Station Sites
            containerRegistry.RegisterForNavigation<EditStationSiteMainPage>(EditStationSiteMainPageViewModel.Tag);
            containerRegistry.RegisterForNavigation<StationSiteHardwarePage>(StationSiteHardwarePageViewModel.Tag);
            containerRegistry.RegisterForNavigation<StationSiteHydraulicPage>(StationSiteHydraulicPageViewModel.Tag);
            containerRegistry.RegisterForNavigation<StationSiteControlChannelPage>(StationSiteControlChannelPageViewModel.Tag);
            containerRegistry.RegisterForNavigation<StationSitePseudoChannelPage>(StationSitePseudoChannelPageViewModel.Tag);
            containerRegistry.RegisterForNavigation<StationSiteModel3DSettingPage>(StationSiteModel3DSettingPageViewModel.Tag);
            // Projects
            containerRegistry.RegisterForNavigation<ProjectListPage>(nameof(ProjectListPageViewModel));
            containerRegistry.RegisterForNavigation<ProjectMainPage>(nameof(ProjectMainPageViewModel));
            // Methods
            containerRegistry.RegisterForNavigation<MethodMainPage>(MethodMainPageViewModel.Tag);
            containerRegistry.RegisterForNavigation<MethodContentPage>(nameof(MethodContentPageViewModel));
            containerRegistry.RegisterForNavigation<MethodBaseInfoPage>(MethodBaseInfoPageViewModel.Tag);
            containerRegistry.RegisterForNavigation<MethodWorkflowSettingPage>(MethodWorkflowSettingPageViewModel.Tag);
            
            containerRegistry.RegisterForNavigation<MethodInterfaceSettingPage>(nameof(MethodInterfaceSettingPageViewModel)); 
             
            // StationSetup  
            containerRegistry.RegisterForNavigation<StationSetupMainPage>(nameof(StationSetupMainPageViewModel));
            containerRegistry.RegisterForNavigation<TransducerCalibrationPage>(nameof(TransducerCalibrationPageViewModel)); 
        }

        /// <summary>
        /// (4)初始化应用程序
        /// </summary>
        protected override void OnInitialized()
        {
            try
            {
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
            GlobalDataManager.Instance.ProcessManager?.Stop();
            GlobalDataManager.Instance.ProcessManager?.Dispose();
            // 关闭主窗口后则释放所有设备连接
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
