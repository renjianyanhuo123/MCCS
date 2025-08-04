using MCCS.Services.NotificationService;
using System.Windows;

namespace MCCS.Components.GlobalNotification
{
    /// <summary>
    /// NotificationContainer.xaml 的交互逻辑
    /// </summary>
    public partial class NotificationContainer
    {
        private INotificationService? _notificationService;
        private bool _isInitialized = false;

        public NotificationContainer()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 关键点: 延迟初始化服务
        /// </summary>
        private void EnsureServiceInitialized()
        {
            if (_isInitialized) return;
            try
            {
                _notificationService = ContainerLocator.Current.Resolve<INotificationService>();
                DataContext = _notificationService;
                _isInitialized = true;
            }
            catch
            {
                // 服务还未准备好，稍后重试
            }
        }

        protected override void OnRender(System.Windows.Media.DrawingContext drawingContext)
        {
            EnsureServiceInitialized();
            base.OnRender(drawingContext);
        }

        private void NotificationItem_CloseClicked(object sender, RoutedEventArgs e)
        {
            EnsureServiceInitialized();

            if (sender is NotificationItemControl { DataContext: Models.NotificationItem notification })
            {
                _notificationService?.Remove(notification);
            }
        }
    }
}
