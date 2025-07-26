namespace MCCS.Components.GlobalNotification.Models
{
    public record NotificationItem
    {
        public string Id { get; init; } = Guid.NewGuid().ToString();
        public string Title { get; init; }
        public string Message { get; init; }
        public NotificationType Type { get; init; }
        public DateTime CreatedAt { get; init; } = DateTime.Now;
        public int AutoHideDelay { get; init; } = 5;
        public bool IsClosable { get; init; } = true;
        public object Tag { get; init; }
    }
}
