namespace MCCS.UserControl.DynamicGrid.FlattenedGrid
{
    /// <summary>
    /// 分割节点
    /// </summary>
    public sealed class SplitterNode : LayoutNode
    {
        public SplitterNode(
            string id,
            double ratio,
            CutDirectionEnum orientation,
            LayoutNode first,
            LayoutNode second) : this(orientation, first, second)
        {
            Id = id;
            Ratio = ratio;
        }

        public SplitterNode(
            CutDirectionEnum orientation,
            LayoutNode first,
            LayoutNode second)
        {
            Direction = orientation;
            LeftNode = first;
            RightNode = second;
            LeftNode.Parent = this;
            RightNode.Parent = this;
        }
        /// <summary>
        /// First 占比
        /// </summary>
        public double Ratio { get; set; } = 0.5;
        /// <summary>
        /// Px
        /// </summary>
        public double SplitterSize { get; set; } = FlattenOperation._splitterThickness;
        /// <summary>
        /// 切分方向
        /// </summary>
        public CutDirectionEnum Direction { get; set; }

        /// <summary>
        /// 左子节点
        /// </summary>
        public LayoutNode LeftNode { get; set; }

        /// <summary>
        /// 右子节点
        /// </summary>
        public LayoutNode RightNode { get; set; }
    }
}
