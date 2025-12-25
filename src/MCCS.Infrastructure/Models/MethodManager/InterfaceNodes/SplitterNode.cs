namespace MCCS.Infrastructure.Models.MethodManager.InterfaceNodes
{
    public class SplitterNode : BaseNode
    {
        /// <summary>
        /// 左侧占比
        /// </summary>
        public double LeftRatio { get; private set; }

        public double RightRatio { get; private set; }

        /// <summary>
        /// Px
        /// </summary>
        public double SplitterSize { get; private set; }

        public SplitterTypeEnum Type { get; private set; }
    }
}
