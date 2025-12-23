using System.Windows;

namespace MCCS.Components.LayoutRootComponents.ViewModels
{
    public class SplitterVerticalLayoutNode : LayoutNode
    {

        public SplitterVerticalLayoutNode(LayoutNode leftNode, LayoutNode rightNode)
        {
            LeftNode = leftNode;
            RightNode = rightNode;
            LeftNode.Parent = this;
            RightNode.Parent = this; 
        } 

        /// <summary>
        /// 左侧占比
        /// </summary>
        private GridLength _leftRatio = new(1, GridUnitType.Star);
        public GridLength LeftRatio { get => _leftRatio; set => SetProperty(ref _leftRatio, value); }

        private GridLength _rightRatio = new(1, GridUnitType.Star);
        public GridLength RightRatio { get => _rightRatio; set => SetProperty(ref _rightRatio, value); }

        /// <summary>
        /// Px
        /// </summary>
        private GridLength _splitterSize = new(2, GridUnitType.Pixel);
        public GridLength SplitterSize { get => _splitterSize; set => SetProperty(ref _splitterSize, value); }
         
    }
}
