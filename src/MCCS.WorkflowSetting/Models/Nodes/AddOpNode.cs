namespace MCCS.WorkflowSetting.Models.Nodes
{
    public class AddOpNode : BaseNode
    {
        public AddOpNode(BaseNode parentNode)
        {
            Name = "Add";
            Width = 20;
            Height = 20;
            Parent = parentNode;
            Type = NodeTypeEnum.Action; 
        } 
    }
}
