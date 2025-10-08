namespace MCCS.WorkflowSetting.Models.Nodes
{
    public class DecisionNode : BaseNode
    {
        public DecisionNode(string name, 
            NodeTypeEnum type, 
            double width,  
            double height,
            int level = 0, 
            int order = -1) : base(name, type, width, height, level, order)
        {
            VirtualWidth = width;
            VirtualHeight = height;
        }

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
