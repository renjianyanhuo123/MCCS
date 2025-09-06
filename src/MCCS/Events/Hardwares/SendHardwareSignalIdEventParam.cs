namespace MCCS.Events.Hardwares
{
    public record SendHardwareSignalIdEventParam
    {
        public long ControllerId { get; init; }
    }
}
