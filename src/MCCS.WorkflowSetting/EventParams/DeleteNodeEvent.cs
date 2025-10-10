namespace MCCS.WorkflowSetting.EventParams
{
    public record DeleteNodeEvent
    {
        public required string NodeId { get; set; }
    }
}
