using System.Collections.ObjectModel;
using MCCS.Components.GlobalNotification.Models;

namespace MCCS.Services.NotificationService
{
    public interface INotificationService
    {
        ObservableCollection<NotificationItem> Notifications { get; }
        void Show(string title, string message, NotificationType type = NotificationType.Info, int autoHideSeconds = 5);
        void Remove(NotificationItem notification);
        void Clear();
    }
}
