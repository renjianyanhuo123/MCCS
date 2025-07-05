namespace MCCS.Events.Controllers
{
    public record InverseControlEventParam
    {
        public required string DeviceId { get; init; }
    }
}
