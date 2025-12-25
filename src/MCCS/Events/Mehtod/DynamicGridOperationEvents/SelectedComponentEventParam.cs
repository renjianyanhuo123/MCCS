namespace MCCS.Events.Mehtod.DynamicGridOperationEvents
{
    public record SelectedComponentEventParam
    {
        public required string SourceId { get; init; }
        public required long NodeId { get; init; }
    }
}
