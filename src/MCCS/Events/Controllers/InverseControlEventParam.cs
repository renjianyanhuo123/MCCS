namespace MCCS.Events.Controllers
{
    public record InverseControlEventParam
    {
        public required long ModelId { get; init; }
    }
}
