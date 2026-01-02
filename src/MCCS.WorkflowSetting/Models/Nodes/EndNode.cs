namespace MCCS.WorkflowSetting.Models.Nodes
{
    public class EndNode : BaseNode
    {

        public EndNode(BaseNode? parentNode = null)
        {
            Name = "End";
            Parent = parentNode;
            Type = NodeTypeEnum.End;
            Width = 56;
            Height = 80;
        }
    }
}
