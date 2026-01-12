namespace MCCS.WorkflowSetting.Models.Nodes
{
    public class AddOpNode : BaseNode
    {
        public AddOpNode(BaseNode? parent)
        {
            Name = "Add";
            Parent = parent;
            Width = 20;
            Height = 20; 
            Type = NodeTypeEnum.Action; 
        } 
    }
}
