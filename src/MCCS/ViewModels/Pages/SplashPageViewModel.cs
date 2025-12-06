using MCCS.Services.StartInitial;
using System.Reflection;
using System.Windows;
using MCCS.Events.StartUp;

namespace MCCS.ViewModels.Pages
{
    public class SplashPageViewModel : BaseViewModel
    {
        public const string Tag = "Splash";

        #region Fields
        private double _progress;
        private string _statusText = string.Empty;
        private string _appName = string.Empty;
        private string _version = string.Empty;
        private readonly ISplashService _splashService; 
        #endregion

        #region Properties
        /// <summary>
        /// 加载进度 (0-100)
        /// </summary>
        public double Progress
        {
            get => _progress;
            set => SetProperty(ref _progress, value);
        }

        /// <summary>
        /// 状态文本
        /// </summary>
        public string StatusText
        {
            get => _statusText;
            set => SetProperty(ref _statusText, value);
        }

        /// <summary>
        /// 应用程序名称
        /// </summary>
        public string AppName
        {
            get => _appName;
            set => SetProperty(ref _appName, value);
        }

        /// <summary>
        /// 版本号
        /// </summary>
        public string Version
        {
            get => _version;
            set => SetProperty(ref _version, value);
        } 
        #endregion

        #region Constructor 
        public SplashPageViewModel(ISplashService splashService,
            IEventAggregator eventAggregator) : base(eventAggregator)
        {
            _splashService = splashService ?? throw new ArgumentNullException(nameof(splashService));  
            Initialize();
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// 初始化
        /// </summary>
        private void Initialize()
        {
            // 获取应用程序信息
            var assembly = Assembly.GetExecutingAssembly();
            AppName = assembly.GetCustomAttribute<AssemblyTitleAttribute>()?.Title ?? "应用程序";
            Version = $"v{assembly.GetName().Version?.ToString(3) ?? "1.0.0"}";

            StatusText = "正在启动...";
            Progress = 0;

            // 开始加载流程
            _ = StartLoadingAsync();
        }

        /// <summary>
        /// 异步加载流程
        /// </summary>
        private async Task StartLoadingAsync()
        {
            try
            {
                await LoadApplicationAsync();
            }
            catch (Exception ex)
            {
                HandleLoadingError(ex);
                _eventAggregator.GetEvent<FinishStartUpNotificationEvent>().Publish(new FinishStartUpNotificationEventParam
                {
                    IsSuccess = false
                });
            }
        }

        /// <summary>
        /// 应用程序加载流程
        /// </summary>
        private async Task LoadApplicationAsync()
        {
            
            var loadingSteps = new (string Status, int Progress, Func<Task> Action)[]
            {  
                ("初始化...", 60, () => _splashService.InitialHardwareDevicesAsync()),
                ("加载模块...", 80, () => Task.Delay(500)),
                ("准备就绪...", 100, () => Task.Delay(500))
            };

            foreach (var (status, progress, action) in loadingSteps)
            {
                StatusText = status;

                // 执行加载步骤
                await action(); 

                // 短暂延迟以显示状态
                await Task.Delay(300);
            }  
            // 加载完成，延迟后关闭启动页
            await Task.Delay(500);
            _eventAggregator.GetEvent<FinishStartUpNotificationEvent>().Publish(new FinishStartUpNotificationEventParam
            {
                IsSuccess = true
            });
        }

        /// <summary>
        /// 处理加载错误
        /// </summary>
        private void HandleLoadingError(Exception ex)
        {
            StatusText = "启动失败";

            // 记录错误日志
            System.Diagnostics.Debug.WriteLine($"启动错误: {ex}");

            // 这里可以显示错误对话框或采取其他错误处理措施
            MessageBox.Show($"应用程序启动失败：{ex.Message}",
                          "错误",
                          MessageBoxButton.OK,
                          MessageBoxImage.Error);

            Application.Current.Shutdown();
        }
        #endregion
    }
}
