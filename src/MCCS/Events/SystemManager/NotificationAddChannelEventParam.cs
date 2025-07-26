namespace MCCS.Events.SystemManager
{
    public record NotificationAddChannelEventParam
    {
        public long ChannelId { get; init; }

        public string ChannelName { get; init; } = string.Empty;
    }
}
