using System.Collections.ObjectModel;

using MCCS.Common.Resources.Models;

namespace MCCS.Common.Resources.Extensions
{
    public interface INotificationService
    {
        ObservableCollection<NotificationItem> Notifications { get; }
        void Show(string title, string message, NotificationType type = NotificationType.Info, int autoHideSeconds = 5);
        void Remove(NotificationItem notification);
        void Clear();
    }
}
