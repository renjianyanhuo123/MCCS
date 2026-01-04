namespace MCCS.Common.Resources.Models
{
    public class NotificationItem : BindableBase
    {
        public string Id { get; init; } = Guid.NewGuid().ToString();
        public required string Title { get; init; }
        public required string Message { get; init; }
        private NotificationType _type;
        public NotificationType Type { get => _type; set => SetProperty(ref _type, value); }
        public DateTime CreatedAt { get; init; } = DateTime.Now;
        public int AutoHideDelay { get; init; } = 5;
        public bool IsClosable { get; init; } = true; 
    }
}
