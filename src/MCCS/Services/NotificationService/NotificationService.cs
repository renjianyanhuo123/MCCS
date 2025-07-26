using MCCS.Components.GlobalNotification.Models;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Threading;

namespace MCCS.Services.NotificationService
{
    public class NotificationService : BindableBase, INotificationService
    {
        public ObservableCollection<NotificationItem> Notifications { get; } = new();
        private readonly DispatcherTimer _timer = new() { Interval = TimeSpan.FromSeconds(1) };

        public NotificationService()
        {
            _timer.Tick += (s, e) => CheckAutoHide();
            _timer.Start();
        }

        public void Show(string title, string message, NotificationType type = NotificationType.Info, int autoHideSeconds = 5)
        {
            var notification = new NotificationItem
            {
                Title = title,
                Message = message,
                Type = type,
                AutoHideDelay = autoHideSeconds
            };

            Notifications.Insert(0, notification);
        }

        public void Remove(NotificationItem notification)
        {
            Notifications.Remove(notification);
        }

        public void Clear()
        {
            Notifications.Clear();
        }

        private void CheckAutoHide()
        {
            var now = DateTime.Now;
            var toRemove = Notifications
                .Where(n => (now - n.CreatedAt).TotalSeconds >= n.AutoHideDelay)
                .ToList();

            foreach (var item in toRemove)
                Remove(item);
        }
    }
}
