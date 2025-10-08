namespace MCCS.WorkflowSetting.Models.Nodes
{
    public class DecisionNode : BaseNode
    { 
        /// <summary>
        /// 虚拟宽度
        /// </summary>
        public double VirtualWidth { get; set; }
        /// <summary>
        /// 虚拟高度
        /// </summary>
        public double VirtualHeight { get; set; }
        /// <summary>
        /// 所有的子节点
        /// </summary>
        public List<BranchNode> Children { get; private set; } = [];
    }
}
