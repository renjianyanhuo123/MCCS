namespace MCCS.Collecter.ControlChannelManagers
{
    public record ControlChannelConfiguration
    {
        public ControlCompletionConfiguration CancellationConfiguration { get; init; }

        public long ChannelId { get; init; }

        public string ChannelName { get; init; } = string.Empty;


    }
}
