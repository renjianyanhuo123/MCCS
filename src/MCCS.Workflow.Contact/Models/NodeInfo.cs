namespace MCCS.Workflow.Contact.Models
{
    public record NodeInfo
    {
        public string Name { get; init; } = string.Empty;
        public string Title { get; init; } = string.Empty; 
        public string TitleBackground { get; init; } = string.Empty;  
        public NodeDisplayTypeEnum DisplayType { get; init; } = NodeDisplayTypeEnum.Step;
    }
}
