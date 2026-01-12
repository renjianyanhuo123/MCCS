namespace MCCS.WorkflowSetting.Models.Nodes
{
    public class EndNode : BaseNode
    { 
        public EndNode()
        {
            Name = "End"; 
            Type = NodeTypeEnum.End;
            Width = 56;
            Height = 80;
        }
    }
}
