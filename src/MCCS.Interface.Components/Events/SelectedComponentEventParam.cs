namespace MCCS.Interface.Components.Events
{
    public record SelectedComponentEventParam
    {
        public required string SourceId { get; init; }
        public required string NodeId { get; init; }
    }
}
