namespace MCCS.WorkflowSetting.Models.Nodes
{
    public class BranchNode : BaseNode 
    {
        public List<BaseNode> Nodes { get; } = [];
    }
}
