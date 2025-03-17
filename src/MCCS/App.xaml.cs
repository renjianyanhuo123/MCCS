using MCCS.Modules;
using MCCS.ViewModels.Pages;
using MCCS.Views;
using MCCS.Views.Pages;
using Microsoft.Extensions.Configuration;
using System.IO;
using System.Windows;

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

            var configuration = builder.Build();

            // 2. 将 IConfiguration 注册到容器
            containerRegistry.RegisterInstance<IConfiguration>(configuration);
            containerRegistry.RegisterForNavigation<HomePage>(HomePageViewModel.Tag);
            containerRegistry.AddRepository(configuration);
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
        }

        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);
        }
    }

}
