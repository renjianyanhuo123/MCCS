namespace MCCS.WorkflowSetting.EventParams
{
    public record DeleteTempPlaceholderNodeEventParam
    {
        public required string SourceId { get; init; }
        public required string PublisherId { get; init; }
    }
}
