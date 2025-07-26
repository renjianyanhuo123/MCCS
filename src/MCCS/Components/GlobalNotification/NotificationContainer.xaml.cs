using MCCS.Services.NotificationService;
using System.Windows;
using System.Windows.Controls;

namespace MCCS.Components.GlobalNotification
{
    /// <summary>
    /// NotificationContainer.xaml 的交互逻辑
    /// </summary>
    public partial class NotificationContainer : UserControl
    {
        private INotificationService _notificationService;
        private bool _isInitialized = false;

        public NotificationContainer()
        {
            InitializeComponent();
        }

        // 延迟初始化服务
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

            if (_notificationService != null &&
                sender is NotificationItemControl item &&
                item.DataContext is Models.NotificationItem notification)
            {
                _notificationService.Remove(notification);
            }
        }
    }
}
