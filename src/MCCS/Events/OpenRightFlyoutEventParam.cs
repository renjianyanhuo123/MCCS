namespace MCCS.Events;

public enum RightFlyoutTypeEnum : int
{
    ControlCommand
}

public class OpenRightFlyoutEventParam
{
    public RightFlyoutTypeEnum Type { get; set; }

    public object Others { get; set; }
}
