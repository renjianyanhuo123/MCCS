namespace MCCS.WorkflowSetting.Models.Nodes
{
    public class DecisionNode : BaseNode
    {
        /// <summary>
        /// 所有的子节点
        /// </summary>
        public List<StepListNodes> Children { get; private set; } = [];

        public DecisionNode()
        {

        }
    }
}
