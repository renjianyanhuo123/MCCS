namespace MCCS.Infrastructure.Models.MethodManager.InterfaceNodes
{
    public class SplitterNode : BaseNode
    {

        public SplitterNode()
        {
        }

        public SplitterNode(
            string id,
            NodeTypeEnum type,
            double leftRatio,
            double rightRatio,
            double splitterSize,
            string? parentId = null,
            string? leftNodeId = null,
            string? rightNodeId = null) : base(id, type, parentId, leftNodeId, rightNodeId)
        {
            LeftRatio = leftRatio;
            RightRatio = rightRatio;
            SplitterSize = splitterSize;
        }

        /// <summary>
        /// 左侧占比
        /// </summary>
        public double LeftRatio { get; set; } 

        public double RightRatio { get; set; } 

        /// <summary>
        /// Px
        /// </summary>
        public double SplitterSize { get; set; } 
    }
}
