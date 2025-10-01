namespace MCCS.Events.Common;

public enum RightFlyoutTypeEnum : int 
{
    WorkflowSetting
}

public record OpenRightFlyoutEventParam
{
    public RightFlyoutTypeEnum Type { get; init; }

    public string Title { get; init; } = string.Empty;
}
