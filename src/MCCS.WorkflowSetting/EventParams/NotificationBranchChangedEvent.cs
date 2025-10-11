namespace MCCS.WorkflowSetting.EventParams
{
    public record NotificationBranchChangedEvent
    {
        public required string Source { get; set; }
    }
}
