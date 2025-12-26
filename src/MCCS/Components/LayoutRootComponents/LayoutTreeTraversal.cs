using MCCS.Components.LayoutRootComponents.ViewModels;
using MCCS.Infrastructure.Models.MethodManager.InterfaceNodes;

namespace MCCS.Components.LayoutRootComponents
{
    public static class LayoutTreeTraversal
    {
        public static List<BaseNode> PostOrderToBaseNodes(LayoutNode root)
        {
            if (root == null)
                return [];

            var result = new List<BaseNode>(128); // 可按规模调整
            var stack = new Stack<LayoutNode>();
            LayoutNode? current = root;
            LayoutNode? lastVisited = null;

            while (current != null || stack.Count > 0)
            {
                // 一直向左下
                while (current != null)
                {
                    stack.Push(current);
                    current = current.LeftNode;
                } 
                var peek = stack.Peek(); 
                // 右子树未访问，转向右
                if (peek.RightNode != null && lastVisited != peek.RightNode)
                {
                    current = peek.RightNode;
                }
                else
                {
                    // 左右子树都处理完，访问当前节点
                    stack.Pop(); 
                    result.Add(CreateBaseNode(peek)); 
                    lastVisited = peek;
                }
            }

            return result;
        }

        private static BaseNode CreateBaseNode(LayoutNode node) =>
            node switch
            {
                SplitterHorizontalLayoutNode splitterHorizontalLayoutNode => new SplitterNode(id: node.Id,
                    type: NodeTypeEnum.SplitterHorizontal, leftRatio: splitterHorizontalLayoutNode.LeftRatio.Value,
                    rightRatio: splitterHorizontalLayoutNode.RightRatio.Value,
                    splitterSize: splitterHorizontalLayoutNode.SplitterSize.Value, parentId: node.Parent?.Id,
                    leftNodeId: node.LeftNode?.Id, rightNodeId: node.RightNode?.Id),
                SplitterVerticalLayoutNode splitterVerticalLayoutNode => new SplitterNode(id: node.Id,
                    type: NodeTypeEnum.SplitterVertical, leftRatio: splitterVerticalLayoutNode.LeftRatio.Value,
                    rightRatio: splitterVerticalLayoutNode.RightRatio.Value,
                    splitterSize: splitterVerticalLayoutNode.SplitterSize.Value, parentId: node.Parent?.Id,
                    leftNodeId: node.LeftNode?.Id, rightNodeId: node.RightNode?.Id),
                CellEditableComponentViewModel cellEditableComponentViewModel => new CellNode(id: node.Id,
                    type: NodeTypeEnum.Cell, nodeId: cellEditableComponentViewModel.NodeId,
                    paramterJson: cellEditableComponentViewModel.ParamterJson, parentId: node.Parent?.Id,
                    leftNodeId: node.LeftNode?.Id, rightNodeId: node.RightNode?.Id),
                _ => throw new InvalidOperationException("未知布局节点类型，无法转换为BaseNode")
            };
    }

}
