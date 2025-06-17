namespace MCCS.Events;

public class ControlEventParam
{
    public required string ChannelId { get; set; }

    public string? ChannelName { get; set; } = string.Empty;
}
