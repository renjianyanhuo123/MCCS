namespace MCCS.WorkflowSetting.Models.Nodes
{
    public class BranchNode : BaseNode 
    {
        public LinkedList<BaseNode> LinkNodes { get; } = [];
    }
}
